using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;
using backend.Services;
using backend.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.Models;

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

    private bool IsAdminOrStaff()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return role == UserRole.admin.ToString() || role == UserRole.staff.ToString();
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
    }

    // GET: api/orders?status=&userId=&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
            string? status,
            Guid? userId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        // Security: If not admin/staff, force filter by current user's ID
        if (!IsAdminOrStaff() && currentUserId.HasValue)
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

        return Ok(result);
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = IsAdminOrStaff() ? null : GetCurrentUserId();
        
        var order = await _service.GetByIdAsync(id, userId, cancellationToken);
        if (order == null) return NotFound(new { message = "Orders not found" });

        return Ok(order);
    }

    // PUT: api/orders/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(new { message = "Order updated successfully" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
