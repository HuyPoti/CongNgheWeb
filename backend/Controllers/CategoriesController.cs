using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]  
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) { _service = service; }

    [HttpGet] public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct) => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id}")] public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct){
        var category = await _service.GetByIdAsync(id, ct);
        return category == null ? NotFound(new { message = "Lỗi"}) : Ok(category);
    }

    [HttpPost] public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct){
        if (!ModelState.IsValid) return BadRequest();
        var category = await _service.CreateAsync(dto, ct);
        if (category == null) return Conflict(new {message = "Slug trùng Parent lỗi"});
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId}, category);
    }

    [HttpPut("{id}")] public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct) {
        var category = await _service.UpdateAsync(id, dto, ct);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id}")] public async Task<ActionResult> Delete(Guid id, CancellationToken ct) {
        if (!await _service.DeleteAsync(id, ct)) return NotFound();
        return NoContent();  
    }
}