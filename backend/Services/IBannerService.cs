using backend.DTOs;

namespace backend.Services;

public interface IBannerService
{
    Task<List<BannerDto>> GetAllAsync(CancellationToken ct);
    Task<List<BannerDto>> GetPublicAsync(CancellationToken ct);
    Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<BannerDto?> CreateAsync(CreateBannerDto dto, CancellationToken ct);
    Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}