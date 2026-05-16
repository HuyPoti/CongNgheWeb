using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<ActionResult<ApiResponse<List<ProductImageDto>>>> Get(Guid productId, CancellationToken ct)
    {
        var result = await _service.GetByProductIdAsync(productId, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ProductImageDto>>> Add(Guid productId, [FromBody] CreateProductImageDto dto, CancellationToken ct)
    {
        var result = await _service.AddAsync(productId, dto, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{imageId}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid imageId, CancellationToken ct)
    {
        await _service.DeleteAsync(imageId, ct);
        return Ok(ApiResponse.Ok(new { message = "Image deleted successfully" }));
    }
}