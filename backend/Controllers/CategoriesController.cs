using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]  
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) { _service = service; }

    [HttpGet] public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(CancellationToken ct) => Ok(ApiResponse.Ok(await _service.GetAllAsync(ct)));

    [HttpGet("{id}")] public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken ct){
        var category = await _service.GetByIdAsync(id, ct);
        return category == null ? NotFound(ApiResponse.Fail("Không tìm thấy danh mục")) : Ok(ApiResponse.Ok(category));
    }

    [HttpPost] 
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct){
        var category = await _service.CreateAsync(dto, ct);
        if (category == null) return Conflict(ApiResponse.Fail("Slug trùng hoặc lỗi cấp cha"));
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId}, ApiResponse.Ok(category));
    }

    [HttpPut("{id}")] 
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct) {
        var category = await _service.UpdateAsync(id, dto, ct);
        return category == null ? NotFound(ApiResponse.Fail("Không tìm thấy danh mục")) : Ok(ApiResponse.Ok(category));
    }

    [HttpDelete("{id}")] 
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct) {
        if (!await _service.DeleteAsync(id, ct)) return NotFound(ApiResponse.Fail("Không tìm thấy danh mục"));
        return Ok(ApiResponse.Ok(new { message = "Đã xóa thành công" }));  
    }
}