using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService service) : ControllerBase
{
    // GET: api/product?keyword=&categoryId=&minPrice=&maxPrice=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll(
        [FromQuery] ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await service.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound(ApiResponse.Fail("Product not found"));
        return Ok(ApiResponse.Ok(product));
    }

    // GET: api/product/{id}/full  →  product + images + specs
    [HttpGet("{id}/full")]
    public async Task<ActionResult<ApiResponse<object>>> GetFull(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetFullByIdAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    // GET: api/product/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await service.GetBySlugAsync(slug, cancellationToken);
        if (product == null) return NotFound(ApiResponse.Fail("Product not found"));
        return Ok(ApiResponse.Ok(product));
    }

    // GET: api/product/slug/{slug}/full
    [HttpGet("slug/{slug}/full")]
    public async Task<ActionResult<ApiResponse<object>>> GetFullBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await service.GetFullBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    // POST: api/product
    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await service.CreateAsync(dto, cancellationToken);
        if (product == null) return BadRequest(ApiResponse.Fail("Invalid data or duplicate slug"));

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, ApiResponse.Ok(product));
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await service.UpdateAsync(id, dto, cancellationToken);
        if (product == null) return BadRequest(ApiResponse.Fail("Update failed"));
        return Ok(ApiResponse.Ok(product));
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Product deleted successfully" }));
    }

    // GET: api/product/client
    // Client-facing: chi hien san pham published (Status = 2)
    // Ho tro: categorySlug, keyword, brandId, minPrice, maxPrice, sortBy, page, pageSize
    [HttpGet("client")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductListItemDto>>>> GetClientProducts(
        [FromQuery] ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await service.GetProductListAsync(query, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }
}