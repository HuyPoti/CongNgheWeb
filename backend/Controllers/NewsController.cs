using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(INewsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NewsDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) => Ok(ApiResponse.Ok(await service.GetAllAsync(page, pageSize, ct)));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NewsDto>>> GetById(Guid id, CancellationToken ct)
    {
        var news = await service.GetByIdAsync(id, ct);
        return news == null ? NotFound(ApiResponse.Fail("Không tìm thấy tin tức")) : Ok(ApiResponse.Ok(news));
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<NewsDto>>> Create([FromBody] CreateNewsDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.NewsId }, ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<NewsDto>>> Update(Guid id, [FromBody] UpdateNewsDto dto, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated == null ? NotFound(ApiResponse.Fail("Không tìm thấy tin tức")) : Ok(ApiResponse.Ok(updated));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result ? Ok(ApiResponse.Ok(new { message = "Đã xóa thành công" })) : NotFound(ApiResponse.Fail("Không tìm thấy tin tức"));
    }
}