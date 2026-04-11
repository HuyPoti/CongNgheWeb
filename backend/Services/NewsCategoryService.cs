using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NewsCategoryService(IUnitOfWork uow, IMapper mapper) : INewsCategoryService
{
    public async Task<List<NewsCategoryDto>> GetAllAsync(CancellationToken ct)
    {
        return await uow.NewsCategories.Query()
            .ProjectTo<NewsCategoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<NewsCategoryDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var item = await uow.NewsCategories.Query().FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        return mapper.Map<NewsCategoryDto>(item);
    }

    public async Task<NewsCategoryDto> CreateAsync(CreateNewsCategoryDto dto, CancellationToken ct)
    {
        var entity = mapper.Map<NewsCategory>(dto);
        uow.NewsCategories.Insert(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<NewsCategoryDto>(entity);
    }

    public async Task<NewsCategoryDto?> UpdateAsync(Guid id, UpdateNewsCategoryDto dto, CancellationToken ct)
    {
        var entity = await uow.NewsCategories.Query().FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (entity == null) return null;
        mapper.Map(dto, entity);
        uow.NewsCategories.Update(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<NewsCategoryDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.NewsCategories.Query().FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (entity == null) return false;

        entity.IsActive = false;
        uow.NewsCategories.Update(entity);
        await uow.SaveAsync(ct);
        return true;
    }
}
