using backend.DTOs;

namespace backend.Services;

public interface INewsCategoryService
{
    Task<List<NewsCategoryDto>> GetAllAsync(CancellationToken ct);
    Task<NewsCategoryDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<NewsCategoryDto> CreateAsync(CreateNewsCategoryDto dto, CancellationToken ct);
    Task<NewsCategoryDto?> UpdateAsync(Guid id, UpdateNewsCategoryDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
