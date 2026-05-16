using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannersController : ControllerBase
{
    private readonly IBannerService _service;

    public BannersController(IBannerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BannerDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<PagedResult<BannerDto>>>> GetPublic(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPublicAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BannerDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _service.GetByIdAsync(id, cancellationToken);
        return banner == null ? NotFound(ApiResponse.Fail("Không tìm thấy banner")) : Ok(ApiResponse.Ok(banner));
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<BannerDto>>> Create([FromBody] CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _service.CreateAsync(dto, cancellationToken);
        if (banner == null)
        {
            return BadRequest(ApiResponse.Fail("Dữ liệu banner không hợp lệ"));
        }

        return CreatedAtAction(nameof(GetById), new { id = banner.BannerId }, ApiResponse.Ok(banner));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<BannerDto>>> Update(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _service.UpdateAsync(id, dto, cancellationToken);
        return banner == null ? NotFound(ApiResponse.Fail("Không tìm thấy banner")) : Ok(ApiResponse.Ok(banner));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _service.DeleteAsync(id, cancellationToken)) return NotFound(ApiResponse.Fail("Không tìm thấy banner"));
        return Ok(ApiResponse.Ok(new { message = "Đã xóa thành công" }));
    }
}
