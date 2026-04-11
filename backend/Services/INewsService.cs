using backend.DTOs;

namespace backend.Services;

public interface INewsService
{
    Task<List<NewsDto>> GetAllAsync(CancellationToken ct);
    Task<NewsDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<NewsDto> CreateAsync(CreateNewsDto dto, CancellationToken ct);
    Task<NewsDto?> UpdateAsync(Guid id, UpdateNewsDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}