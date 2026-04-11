using backend.DTOs;

namespace backend.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, CancellationToken ct);
    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}