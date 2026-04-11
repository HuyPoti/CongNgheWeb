

using backend.DTOs;
namespace backend.Services;

public interface IProductImageService
{
    Task<List<ProductImageDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductImageDto> AddAsync(Guid productId, CreateProductImageDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(Guid imageId, CancellationToken cancellationToken);
}