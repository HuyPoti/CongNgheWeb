using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;
using backend.Services;
using backend.DTOs;

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

    // GET: api/orders?status=&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
            string? status,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(
            status,
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
        var order = await _service.GetByIdAsync(id, cancellationToken);
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
