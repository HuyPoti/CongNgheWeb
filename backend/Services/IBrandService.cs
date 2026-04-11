using backend.DTOs;

namespace backend.Services;

public interface IBrandService
{
    Task<List<BrandDto>> GetAllAsync(CancellationToken ct);
    Task<BrandDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<BrandDto?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<BrandDto?> CreateAsync(CreateBrandDto dto, CancellationToken ct);
    Task<BrandDto?> UpdateAsync(Guid id, UpdateBrandDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
