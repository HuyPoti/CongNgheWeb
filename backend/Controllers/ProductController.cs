using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;

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

    // GET: api/product?keyword=&categoryId=&minPrice=&maxPrice=&page=&pageSize=
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
            keyword, categoryId, minPrice, maxPrice,
            cancellationToken, page, pageSize);
        return Ok(result);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    // GET: api/product/{id}/full  →  product + images + specs
    [HttpGet("{id}/full")]
    public async Task<IActionResult> GetFull(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetFullByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    // GET: api/product/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await _service.GetBySlugAsync(slug, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    // GET: api/product/slug/{slug}/full
    [HttpGet("slug/{slug}/full")]
    public async Task<IActionResult> GetFullBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _service.GetFullBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    // POST: api/product
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await _service.CreateAsync(dto, cancellationToken);
        if (product == null) return BadRequest("Invalid data or duplicate slug");

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _service.UpdateAsync(id, dto, cancellationToken);
        if (product == null) return BadRequest("Update failed");
        return Ok(product);
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    // GET: api/product/client
    // Client-facing: chi hien san pham published (Status = 2)
    // Ho tro: categorySlug, keyword, brandId, minPrice, maxPrice, sortBy, page, pageSize
    [HttpGet("client")]
    public async Task<IActionResult> GetClientProducts(
        [FromQuery] string? categorySlug,
        [FromQuery] string? keyword,
        [FromQuery] Guid? brandId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _service.GetProductListAsync(
            cancellationToken, page, pageSize,
            categorySlug, keyword, brandId,
            minPrice, maxPrice, sortBy);
        return Ok(result);
    }
}