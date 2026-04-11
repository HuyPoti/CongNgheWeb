using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService(
    IUnitOfWork uow, 
    IMapper mapper,
    IProductImageService imageService,
    IProductSpecService specService) : IProductService
{
    // GET ALL ASYNC
    public async Task<PagedResult<ProductDto>> GetAllAsync(
        string? keyword,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken,
        int page,
        int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        pageSize = pageSize > 20 ? 20 : pageSize;

        var query = uow.Products.Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            var trimmed = keyword.Trim();

            query = trimmed.Length <= 3
                ? query.Where(p => EF.Functions.ILike(p.Name, $"{trimmed}%"))
                : query.Where(p => EF.Functions.ILike(p.Name, $"%{trimmed}%"));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.RegularPrice >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.RegularPrice <= maxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductFullDto> GetFullByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await GetByIdAsync(id, cancellationToken);

        var images = await imageService.GetByProductIdAsync(id, cancellationToken);
        var specs = await specService.GetByProductIdAsync(id, cancellationToken);

        return new ProductFullDto
        {
            Product = product!,
            Images = images,
            Specs = specs
        };
    }

    // GET BY ID ASYNC
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
            
        if (product == null) throw new NotFoundException("Product not found");
        return product;
    }


    // GET BY SLUG
   public async Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await uow.Products
            .Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
            
        if (product == null) throw new NotFoundException("Product not found");
        return product;
    }

    // CREATE
    public async Task<ProductDto?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken)
    {
        var category = await uow.Categories.GetByIdAsync<CategoryDto>(dto.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category not found");

        var brand = await uow.Brands.GetByIdAsync<BrandDto>(dto.BrandId, cancellationToken);
        if (brand == null)
            throw new NotFoundException("Brand not found");
            
        var existing = await uow.Products.Query()
            .AnyAsync(p => p.Slug == dto.Slug, cancellationToken);

        if (existing)
            throw new BadRequestException("Slug already exists");

        var product = mapper.Map<Product>(dto);

        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.Status = dto.Status.HasValue ? (int)dto.Status.Value : 1; 
        // 1: draft, 2: published

        uow.Products.Insert(product);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<ProductDto>(product);
    }


    // UPDATE
    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query().FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
        if (product == null) throw new NotFoundException("Product not found");
        
        if (!string.IsNullOrEmpty(dto.Slug)) {
            var exists = await uow.Products
                .Query()
                .AnyAsync(p => p.Slug == dto.Slug && p.ProductId != id, cancellationToken);
            if (exists) throw new BadRequestException("Slug already exists");
        }
        
        if (dto.CategoryId.HasValue) {
            var category = await uow.Categories.GetByIdAsync<CategoryDto>(dto.CategoryId.Value, cancellationToken);
            if (category == null) throw new NotFoundException("Category not found");
        }

        if (dto.BrandId.HasValue) {
            var brand = await uow.Brands.GetByIdAsync<BrandDto>(dto.BrandId.Value, cancellationToken);
            if (brand == null) throw new NotFoundException("Brand not found");
        }

        mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<ProductDto>(product);
    }

    // DELETE
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query().FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
        if (product == null) throw new NotFoundException("Product not found");

        product.Status = 1; // 1: Inactive (Soft Deleted)
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);
        await uow.SaveAsync(cancellationToken);
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductListAsync(
        CancellationToken cancellationToken,
        int page,
        int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 12 : pageSize;
        pageSize = pageSize > 50 ? 50 : pageSize;

        var query = uow.Products.Query()
            .Where(p => p.Status == 2) // Chỉ khách hàng mới thấy sản phẩm Hoạt động
            .Include(p => p.Category)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.ProductId,
                Name = p.Name,
                Price = p.SalePrice ?? p.RegularPrice,
                CategoryName = p.Category.Name,

                ThumbnailUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
                    ?? p.Images
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}