using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<List<BannerDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("public")]
    public async Task<ActionResult<List<BannerDto>>> GetPublic(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetPublicAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BannerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var banner = await _service.GetByIdAsync(id, cancellationToken);
        return banner == null ? NotFound(new { message = "Không tìm thấy banner" }) : Ok(banner);
    }

    [HttpPost]
    public async Task<ActionResult<BannerDto>> Create([FromBody] CreateBannerDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Dữ liệu banner không hợp lệ" });
        }

        var banner = await _service.CreateAsync(dto, cancellationToken);
        if (banner == null)
        {
            return BadRequest(new { message = "Dữ liệu banner không hợp lệ" });
        }

        return CreatedAtAction(nameof(GetById), new { id = banner.BannerId }, banner);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BannerDto>> Update(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var banner = await _service.UpdateAsync(id, dto, cancellationToken);
        return banner == null ? NotFound(new { message = "Không tìm thấy banner" }) : Ok(banner);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _service.DeleteAsync(id, cancellationToken)) return NotFound(new { message = "Không tìm thấy banner" });
        return NoContent();
    }
}