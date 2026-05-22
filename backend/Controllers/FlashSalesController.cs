using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/flash-sales")]
[Authorize(Roles = "admin")]
public class FlashSalesController : ControllerBase
{
    private readonly IFlashSaleService _flashSaleService;

    public FlashSalesController(IFlashSaleService flashSaleService)
    {
        _flashSaleService = flashSaleService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FlashSaleDto>>> Create([FromBody] CreateFlashSaleDto dto, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FlashSaleDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var res = await _flashSaleService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FlashSaleDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FlashSaleDto>>> Update(Guid id, [FromBody] UpdateFlashSaleDto dto, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<FlashSaleDto>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(res, "Flash sale deactivated successfully"));
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<FlashSaleDto>>> GetActive(CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.GetActiveAsync(cancellationToken);
        if (res == null) return NotFound(ApiResponse.Fail("No active flash sale"));
        return Ok(ApiResponse.Ok(res));
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<ApiResponse<FlashSaleItemDto>>> AddItem(Guid id, [FromBody] CreateFlashSaleItemDto dto, CancellationToken cancellationToken)
    {
        var res = await _flashSaleService.AddItemAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpDelete("{id}/items/{productId}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveItem(Guid id, Guid productId, CancellationToken cancellationToken)
    {
        await _flashSaleService.RemoveItemAsync(id, productId, null, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Đã xóa sản phẩm khỏi flash sale" }));
    }
}
