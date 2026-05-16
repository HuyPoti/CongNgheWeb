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
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        return Guid.TryParse(idStr, out var guid) ? guid : null;
    }



    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
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

        // Security: If not admin/staff, force filter by current user's ID
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
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
        bool isAdminOrStaff = User.IsInRole("admin") || User.IsInRole("staff");
        var userId = isAdminOrStaff ? null : GetCurrentUserId();
        
        var order = await _service.GetByIdAsync(id, userId, cancellationToken);
        if (order == null) return NotFound(ApiResponse.Fail("Order not found"));

        return Ok(ApiResponse.Ok(order));
    }

    // PUT: api/orders/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] UpdateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId() ?? Guid.Empty;
        await _service.UpdateAsync(id, dto, currentUserId, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Order updated successfully" }));
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
