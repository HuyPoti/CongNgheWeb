using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;
using backend.Services;
using backend.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    private Guid? GetCurrentUserId()
    {
        // var claimsInfo = string.Join("\n", User.Claims.Select(c => $"{c.Type}: {c.Value}"));
        // System.IO.File.WriteAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "claims_debug.txt"), claimsInfo);

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                 ?? User.FindFirstValue("sub")
                 ?? User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
        
        return Guid.TryParse(idStr, out var guid) ? guid : null;
    }



    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.UserId ??= GetCurrentUserId();
        var order = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, ApiResponse.Ok(order));
    }

    // GET: api/orders?status=&userId=&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetAll(
            string? status,
            Guid? userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        // Security: If not admin/staff/warehouse, force filter by current user's ID
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff") || User.IsInRole("warehouse");
        if (!isAdminOrStaff && currentUserId.HasValue)
        {
            userId = currentUserId;
        }

        var result = await _service.GetAllAsync(
            status,
            userId,
            page,
            pageSize,
            cancellationToken
        );

        return Ok(ApiResponse.Ok(result));
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff") || User.IsInRole("warehouse");
        var userId = isAdminOrStaff ? null : GetCurrentUserId();
        
        var order = await _service.GetByIdAsync(id, userId, cancellationToken);
        if (order == null) return NotFound(ApiResponse.Fail("Order not found"));

        return Ok(ApiResponse.Ok(order));
    }

    // PUT: api/orders/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff,warehouse")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] UpdateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        // Chỉ Admin mới được xác nhận "Đã giao" (delivered)
        bool isAdmin = User.IsInRole("admin");
        if (!isAdmin && dto.Status?.ToLower() == "delivered")
            return Forbid();

        var currentUserId = GetCurrentUserId() ?? Guid.Empty;
        await _service.UpdateAsync(id, dto, currentUserId, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Order updated successfully" }));
    }

    // POST: api/orders/{id}/mark-delivered (Admin only)
    [HttpPost("{id}/mark-delivered")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> MarkDelivered(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId() ?? Guid.Empty;
        var dto = new UpdateOrderDto { Status = "delivered" };
        await _service.UpdateAsync(id, dto, currentUserId, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Order marked as delivered" }));
    }

    // POST: api/orders/{id}/cancel
    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(
        Guid id,
        [FromBody] CancelOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        await _service.CancelAsync(id, dto, currentUserId, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Order cancelled successfully" }));
    }

    // GET: api/orders/{id}/history
    [HttpGet("{id}/history")]
    public async Task<ActionResult<ApiResponse<List<OrderStatusHistoryDto>>>> GetStatusHistory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var history = await _service.GetStatusHistoryAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(history));
    }
}
