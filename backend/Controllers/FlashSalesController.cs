using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class FlashSalesController : ControllerBase
{
    private readonly IFlashSaleService _flashSaleService;

    public FlashSalesController(IFlashSaleService flashSaleService)
    {
        _flashSaleService = flashSaleService;
    }

    [HttpPost]
    public async Task<ActionResult<FlashSaleDto>> Create([FromBody] CreateFlashSaleDto dto, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.CreateAsync(dto, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<FlashSaleDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var res = await _flashSaleService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<FlashSaleDto?>> GetActive(CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.GetActiveAsync(cancellationToken);
        if (res == null) return NotFound(new { message = "No active flash sale" });
        return Ok(res);
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<FlashSaleItemDto>> AddItem(Guid id, [FromBody] CreateFlashSaleItemDto dto, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.AddItemAsync(id, dto, cancellationToken);
        return Ok(res);
    }

    [HttpDelete("{id}/items/{productId}")]
    public async Task<ActionResult> RemoveItem(Guid id, Guid productId, CancellationToken cancellationToken)
    {
        await _flashSaleService.RemoveItemAsync(id, productId, null, cancellationToken);
        return NoContent();
    }
}
