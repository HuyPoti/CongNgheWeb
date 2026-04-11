using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]  
public class BrandsController : ControllerBase
{
    private readonly IBrandService _service;
    public BrandsController(IBrandService service) { _service = service; }

    [HttpGet] public async Task<ActionResult<List<BrandDto>>> GetAll(CancellationToken cancellationToken) => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{id}")] public async Task<ActionResult<BrandDto>> GetById(Guid id, CancellationToken cancellationToken){
        var brand = await _service.GetByIdAsync(id, cancellationToken);
        return brand == null ? NotFound(new { message = "Không tìm thấy thương hiệu"}) : Ok(brand);
    }

    [HttpGet("slug/{slug}")] public async Task<ActionResult<BrandDto>> GetBySlug(string slug, CancellationToken cancellationToken){
        var brand = await _service.GetBySlugAsync(slug, cancellationToken);
        return brand == null ? NotFound(new { message = "Không tìm thấy thương hiệu"}) : Ok(brand);
    }

    [HttpPost] public async Task<ActionResult<BrandDto>> Create([FromBody] CreateBrandDto dto, CancellationToken cancellationToken){
        if (!ModelState.IsValid) return BadRequest();
        var brand = await _service.CreateAsync(dto, cancellationToken);
        if (brand == null) return Conflict(new {message = "Slug đã tồn tại"});
        return CreatedAtAction(nameof(GetById), new { id = brand.BrandId}, brand);
    }

    [HttpPut("{id}")] public async Task<ActionResult<BrandDto>> Update(Guid id, [FromBody] UpdateBrandDto dto, CancellationToken cancellationToken) {
        var brand = await _service.UpdateAsync(id, dto, cancellationToken);
        return brand == null ? NotFound(new { message = "Không tìm thấy hoặc slug trùng"}) : Ok(brand);
    }

    [HttpDelete("{id}")] public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken) {
        if (!await _service.DeleteAsync(id, cancellationToken)) return NotFound();
        return NoContent();  
    }
}
