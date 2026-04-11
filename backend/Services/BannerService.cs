using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BannerService(IUnitOfWork uow, IMapper mapper) : IBannerService
{
    public async Task<List<BannerDto>> GetAllAsync(CancellationToken ct) 
    {
        var banners = await uow.Banners.Query()
            .ProjectTo<BannerDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return banners;
    }

    public async Task<List<BannerDto>> GetPublicAsync(CancellationToken ct)
    {
        var banners = await uow.Banners.GetAsync<BannerDto>(b => b.IsActive, ct);
        return banners.Where(IsPublicBanner).ToList();
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