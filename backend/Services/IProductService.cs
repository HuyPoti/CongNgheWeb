using backend.DTOs;

namespace backend.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken
    );

    Task<ProductFullDto> GetFullByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductFullDto> GetFullBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ProductDto?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ProductListItemDto>> GetProductListAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken
    );
}