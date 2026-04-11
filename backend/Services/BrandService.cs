using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace backend.Services;

public class BrandService(IUnitOfWork uow, IMapper mapper) : IBrandService
{
    public async Task<List<BrandDto>> GetAllAsync(CancellationToken ct)
    {
        var brands = await uow.Brands.Query()
            .ProjectTo<BrandDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return brands;
    }
    public async Task<BrandDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await uow.Brands.GetByIdAsync<BrandDto>(id, ct);
    }
    public async Task<BrandDto?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var result = await uow.Brands.GetAsync<BrandDto>(b => b.Slug == slug, ct);
        return result.FirstOrDefault();
    }
    public async Task<BrandDto?> CreateAsync(CreateBrandDto dto, CancellationToken ct)
    {
        var existing = await uow.Brands.GetAsync<BrandDto>(b => b.Slug == dto.Slug, ct);
        if (existing.Any()) return null;

        var entity = mapper.Map<Brand>(dto);
        uow.Brands.Insert(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<BrandDto>(entity);
    }
    public async Task<BrandDto?> UpdateAsync(Guid id, UpdateBrandDto dto, CancellationToken ct)
    {
        var entity = await uow.Brands.Query().FirstOrDefaultAsync(b => b.BrandId == id, ct);
        if (entity == null) return null;

        if (dto.Slug != null)
        {
            var existing = await uow.Brands.Query().FirstOrDefaultAsync(b => b.Slug == dto.Slug, ct);
            if (existing != null && existing.BrandId != id) return null;
        }

        mapper.Map(dto, entity);
        uow.Brands.Update(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<BrandDto>(entity);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) {
        var entity = await uow.Brands.Query().FirstOrDefaultAsync(b => b.BrandId == id, ct);
        if (entity == null) return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Brands.Update(entity);
        await uow.SaveAsync(ct);
        return true;
    }

}
