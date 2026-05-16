using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]  
public class BrandsController : ControllerBase
{
    private readonly IBrandService _service;
    public BrandsController(IBrandService service) { _service = service; }

    [HttpGet] public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetAll(CancellationToken cancellationToken) => Ok(ApiResponse.Ok(await _service.GetAllAsync(cancellationToken)));

    [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(Guid id, CancellationToken cancellationToken){
        var brand = await _service.GetByIdAsync(id, cancellationToken);
        return brand == null ? NotFound(ApiResponse.Fail("Không tìm thấy thương hiệu")) : Ok(ApiResponse.Ok(brand));
    }

    [HttpGet("slug/{slug}")] public async Task<ActionResult<ApiResponse<BrandDto>>> GetBySlug(string slug, CancellationToken cancellationToken){
        var brand = await _service.GetBySlugAsync(slug, cancellationToken);
        return brand == null ? NotFound(ApiResponse.Fail("Không tìm thấy thương hiệu")) : Ok(ApiResponse.Ok(brand));
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Create([FromBody] CreateBrandDto dto, CancellationToken cancellationToken){
        var brand = await _service.CreateAsync(dto, cancellationToken);
        if (brand == null) return Conflict(ApiResponse.Fail("Slug đã tồn tại"));
        return CreatedAtAction(nameof(GetById), new { id = brand.BrandId}, ApiResponse.Ok(brand));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Update(Guid id, [FromBody] UpdateBrandDto dto, CancellationToken cancellationToken) {
        var brand = await _service.UpdateAsync(id, dto, cancellationToken);
        return brand == null ? NotFound(ApiResponse.Fail("Không tìm thấy hoặc slug trùng")) : Ok(ApiResponse.Ok(brand));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) {
        if (!await _service.DeleteAsync(id, cancellationToken)) return NotFound(ApiResponse.Fail("Không tìm thấy thương hiệu"));
        return Ok(ApiResponse.Ok(new { message = "Đã xóa thành công" }));  
    }
}
