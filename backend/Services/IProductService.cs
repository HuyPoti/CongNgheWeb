using backend.DTOs;

namespace backend.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(
        string? keyword,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken,
        int page,
        int pageSize
    );

    Task<ProductFullDto> GetFullByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductFullDto> GetFullBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ProductDto?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ProductListItemDto>> GetProductListAsync(
        CancellationToken cancellationToken,
        int page,
        int pageSize,
        string? categorySlug = null,
        string? keyword = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null
    );
}