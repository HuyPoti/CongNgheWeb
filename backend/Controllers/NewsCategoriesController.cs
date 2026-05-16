using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/news-categories")]
public class NewsCategoriesController(INewsCategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NewsCategoryDto>>>> GetAll(CancellationToken ct) => Ok(ApiResponse.Ok(await service.GetAllAsync(ct)));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NewsCategoryDto>>> GetById(Guid id, CancellationToken ct)
    {
        var item = await service.GetByIdAsync(id, ct);
        return item == null ? NotFound(ApiResponse.Fail("Không tìm thấy danh mục tin tức")) : Ok(ApiResponse.Ok(item));
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<NewsCategoryDto>>> Create([FromBody] CreateNewsCategoryDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<NewsCategoryDto>>> Update(Guid id, [FromBody] UpdateNewsCategoryDto dto, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated == null ? NotFound(ApiResponse.Fail("Không tìm thấy danh mục tin tức")) : Ok(ApiResponse.Ok(updated));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result ? Ok(ApiResponse.Ok(new { message = "Đã xóa thành công" })) : NotFound(ApiResponse.Fail("Không tìm thấy danh mục tin tức"));
    }
}
