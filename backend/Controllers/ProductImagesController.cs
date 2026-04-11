using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/products/{productId}/images")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _service;

    public ProductImagesController(IProductImageService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid productId, CancellationToken ct)
    {
        var result = await _service.GetByProductIdAsync(productId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid productId, [FromBody] CreateProductImageDto dto, CancellationToken ct)
    {
        var result = await _service.AddAsync(productId, dto, ct);
        return Ok(result);
    }

    [HttpDelete("{imageId}")]
    public async Task<IActionResult> Delete(Guid imageId, CancellationToken ct)
    {
        await _service.DeleteAsync(imageId, ct);
        return NoContent();
    }
}