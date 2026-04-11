using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("api/products/{productId}/specs")]
public class ProductSpecController : ControllerBase
{
    private readonly IProductSpecService _service;

    public ProductSpecController(IProductSpecService service)
    {
        _service = service;
    }
    [HttpGet]
    public async Task<IActionResult> Get(Guid productId, CancellationToken ct)
        => Ok(await _service.GetByProductIdAsync(productId, ct));

    [HttpPost]
    public async Task<IActionResult> Add(Guid productId, CreateProductSpecDto dto, CancellationToken ct)
        => Ok(await _service.AddAsync(productId, dto, ct));

    [HttpPut("{specId}")]
    public async Task<IActionResult> Update(Guid specId, CreateProductSpecDto dto, CancellationToken ct)
        => Ok(await _service.UpdateAsync(specId, dto, ct));

    [HttpDelete("{specId}")]
    public async Task<IActionResult> Delete(Guid specId, CancellationToken ct)
    {
        await _service.DeleteAsync(specId, ct);
        return NoContent();
    }
}