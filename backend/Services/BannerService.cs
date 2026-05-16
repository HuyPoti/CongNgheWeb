using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using backend.Extensions;

namespace backend.Services;

public class BannerService(IUnitOfWork uow, IMapper mapper) : IBannerService
{
    public async Task<PagedResult<BannerDto>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        return await uow.Banners.Query()
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ProjectTo<BannerDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(page, pageSize, ct);
    }
 
    public async Task<PagedResult<BannerDto>> GetPublicAsync(int page, int pageSize, CancellationToken ct)
    {
        var today = DateTime.UtcNow;
        return await uow.Banners.Query()
            .Where(b => b.IsActive &&
                        (!b.StartDate.HasValue || b.StartDate.Value <= today) &&
                        (!b.EndDate.HasValue || b.EndDate.Value >= today))
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ProjectTo<BannerDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(page, pageSize, ct);
    }

    public async Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var banner = await uow.Banners.GetByIdAsync<BannerDto>(id, ct);
        return banner;
    }

    public async Task<BannerDto?> CreateAsync(CreateBannerDto dto, CancellationToken ct)
    {   
        if (!IsValidBannerWindow(dto.StartDate, dto.EndDate)) return null;
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.ImageUrl)) return null;

        var entity = mapper.Map<Banner>(dto);
        
        // Đảm bảo UTC cho PostgreSQL
        if (entity.StartDate.HasValue) entity.StartDate = DateTime.SpecifyKind(entity.StartDate.Value, DateTimeKind.Utc);
        if (entity.EndDate.HasValue) entity.EndDate = DateTime.SpecifyKind(entity.EndDate.Value, DateTimeKind.Utc);
        
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        uow.Banners.Insert(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<BannerDto>(entity);
    }

    public async Task<BannerDto?> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken ct)
    {
        var entity = await uow.Banners.Query().FirstOrDefaultAsync(b => b.BannerId == id, ct);
        if (entity == null) return null;

        if (!IsValidBannerWindow(dto.StartDate ?? entity.StartDate, dto.EndDate ?? entity.EndDate)) return null;

        mapper.Map(dto, entity);

        // Đảm bảo UTC cho PostgreSQL
        if (entity.StartDate.HasValue) entity.StartDate = DateTime.SpecifyKind(entity.StartDate.Value, DateTimeKind.Utc);
        if (entity.EndDate.HasValue) entity.EndDate = DateTime.SpecifyKind(entity.EndDate.Value, DateTimeKind.Utc);
        
        entity.UpdatedAt = DateTime.UtcNow;

        uow.Banners.Update(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<BannerDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.Banners.Query().FirstOrDefaultAsync(b => b.BannerId == id, ct);
        if (entity == null) return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Banners.Update(entity);
        await uow.SaveAsync(ct);
        return true;
    }

    private static bool IsPublicBanner(BannerDto banner)
    {
        if (!banner.IsActive) return false;
        var today = DateTime.UtcNow.Date;
        if (banner.StartDate.HasValue && banner.StartDate.Value.Date > today) return false;
        if (banner.EndDate.HasValue && banner.EndDate.Value.Date < today) return false;
        return true;
    }

    private static bool IsValidBannerWindow(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            return startDate.Value.Date <= endDate.Value.Date;
        }
        return true;
    }
}