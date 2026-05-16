using backend.DTOs;

namespace backend.Services;

public interface IBannerService
{
    Task<PagedResult<BannerDto>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<PagedResult<BannerDto>> GetPublicAsync(int page, int pageSize, CancellationToken ct);
    Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<BannerDto?> CreateAsync(CreateBannerDto dto, CancellationToken ct);
    Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}