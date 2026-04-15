using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CategoryService(IUnitOfWork uow, IMapper mapper) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await uow.Categories.Query()
            .Where(c => c.IsActive)
            .ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return items;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await uow.Categories.GetByIdAsync<CategoryDto>(id, cancellationToken);
        return category;
    }

    public async Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var existing = await uow.Categories.GetAsync<CategoryDto>(c => c.Slug == dto.Slug, cancellationToken);
        if (existing.Any()) return null;
        if (dto.ParentId.HasValue && await uow.Categories.GetByIdAsync<CategoryDto>(dto.ParentId.Value, cancellationToken) == null) return null;

        var entity = mapper.Map<Category>(dto);
        uow.Categories.Insert(entity); 
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var entity = await uow.Categories.Query().FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (entity == null) return null;
        
        if (dto.Slug != null)
        {
            var existing = await uow.Categories.Query().FirstOrDefaultAsync(c => c.Slug == dto.Slug, cancellationToken);
            if (existing != null && existing.CategoryId != id) return null;
        }

        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Categories.Update(entity);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<CategoryDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await uow.Categories.Query().FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (entity == null) return false;

        entity.IsActive = false;
        uow.Categories.Update(entity);
        await uow.SaveAsync(cancellationToken);
        return true;
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var result = await uow.Categories.GetAsync<CategoryDto>(c => c.Slug == slug, cancellationToken);
        return result.FirstOrDefault();
    }
}