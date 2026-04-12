using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(INewsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var news = await service.GetByIdAsync(id, ct);
        return news == null ? NotFound() : Ok(news);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNewsDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.NewsId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNewsDto dto, CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result ? NoContent() : NotFound();
    }
}