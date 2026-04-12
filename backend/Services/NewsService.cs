using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NewsService(IUnitOfWork uow, IMapper mapper) : INewsService
{
    public async Task<List<NewsDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await uow.News.Query()
            .Include(n => n.Category)
            .Include(n => n.Author)
            .ProjectTo<NewsDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return items;
    }

    public async Task<NewsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await uow.News.Query().Include(n => n.Category).Include(n => n.Author).FirstOrDefaultAsync(n => n.NewsId == id, cancellationToken);
        return mapper.Map<NewsDto>(item);
    }

    public async Task<NewsDto> CreateAsync(CreateNewsDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<News>(dto);
        uow.News.Insert(entity);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<NewsDto>(entity);
    }

    public async Task<NewsDto?> UpdateAsync(Guid id, UpdateNewsDto dto, CancellationToken cancellationToken)
    {
        var entity = await uow.News.Query().FirstOrDefaultAsync(n => n.NewsId == id, cancellationToken);
        if (entity == null) return null;
        mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        uow.News.Update(entity);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<NewsDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await uow.News.Query().FirstOrDefaultAsync(n => n.NewsId == id, cancellationToken);
        if (entity == null) return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.News.Update(entity);
        await uow.SaveAsync(cancellationToken);
        return true;
    }
}