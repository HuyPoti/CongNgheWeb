using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using backend.MapperProfiles;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    // GET: api/products?keyword=abc&categoryId=...
    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? keyword,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await _service.GetAllAsync(
            keyword,
            categoryId,
            minPrice,
            maxPrice,
            cancellationToken,
            page,
            pageSize
        );

        return Ok(result);
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpGet("{id}/full")]
    public async Task<IActionResult> GetFull(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetFullByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    // GET: api/products/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await _service.GetBySlugAsync(slug, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _service.CreateAsync(dto, cancellationToken);
        if (product == null) return BadRequest("Invalid data or duplicate slug");

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    // PUT: api/products/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _service.UpdateAsync(id, dto, cancellationToken);
        if (product == null) return BadRequest("Update failed");

        return Ok(product);
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // GET: api/product/client
    [HttpGet("client")]
    public async Task<IActionResult> GetClientProducts(
        [FromQuery] string? categorySlug,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 12)
    {
        var result = await _service.GetProductListAsync(
            cancellationToken,
            page,
            pageSize,
            categorySlug
        );

        return Ok(result);
    }
}