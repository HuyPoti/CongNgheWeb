

using backend.DTOs;
namespace backend.Services;

public interface IProductSpecService
{
    Task<List<ProductSpecDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductSpecDto> AddAsync(Guid productId, CreateProductSpecDto dto, CancellationToken cancellationToken);

    Task<ProductSpecDto> UpdateAsync(Guid specId, CreateProductSpecDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(Guid specId, CancellationToken cancellationToken);
}