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
    IProductImageService imageService) : IProductService
{
    // ── Admin: danh sach co filter ─────────────────────────────────
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
        pageSize = Math.Clamp(pageSize, 1, 20);

        var query = uow.Products.Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = kw.Length <= 3
                ? query.Where(p => EF.Functions.ILike(p.Name, $"{kw}%"))
                : query.Where(p => EF.Functions.ILike(p.Name, $"%{kw}%"));
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

    // ── Get full (product + images + specs) ───────────────────────
    public async Task<ProductFullDto> GetFullByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        var images = await imageService.GetByProductIdAsync(id, cancellationToken);

        return new ProductFullDto
        {
            Product = product!,
            Images = images
        };
    }

    public async Task<ProductFullDto> GetFullBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await GetBySlugAsync(slug, cancellationToken);
        var images = await imageService.GetByProductIdAsync(product!.ProductId, cancellationToken);

        return new ProductFullDto
        {
            Product = product,
            Images = images
        };
    }

    // ── Get by ID ──────────────────────────────────────────────────
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product not found");

        return product;
    }

    // ── Get by Slug ────────────────────────────────────────────────
    public async Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query()
            .Where(p => p.Status != 3)
            .Include(p => p.Category)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product not found");

        return product;
    }

    // ── Create ─────────────────────────────────────────────────────
    public async Task<ProductDto?> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken)
    {
        var category = await uow.Categories.GetByIdAsync<CategoryDto>(dto.CategoryId, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category not found");

        var brand = await uow.Brands.GetByIdAsync<BrandDto>(dto.BrandId, cancellationToken);
        if (brand == null)
            throw new NotFoundException("Brand not found");

        var slugExists = await uow.Products.Query()
            .AnyAsync(p => p.Slug == dto.Slug, cancellationToken);

        if (slugExists)
            throw new BadRequestException("Slug already exists");

        var product = mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.Status = dto.Status ?? 1;

        uow.Products.Insert(product);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<ProductDto>(product);
    }

    // ── Update ─────────────────────────────────────────────────────
    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query()
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product not found");

        if (!string.IsNullOrEmpty(dto.Slug))
        {
            var exists = await uow.Products.Query()
                .AnyAsync(p => p.Slug == dto.Slug && p.ProductId != id, cancellationToken);
            if (exists)
                throw new BadRequestException("Slug already exists");
        }

        if (dto.CategoryId.HasValue)
        {
            var cat = await uow.Categories.GetByIdAsync<CategoryDto>(dto.CategoryId.Value, cancellationToken);
            if (cat == null)
                throw new NotFoundException("Category not found");
        }

        if (dto.BrandId.HasValue)
        {
            var brand = await uow.Brands.GetByIdAsync<BrandDto>(dto.BrandId.Value, cancellationToken);
            if (brand == null)
                throw new NotFoundException("Brand not found");
        }

        mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);
        await uow.SaveAsync(cancellationToken);
        return mapper.Map<ProductDto>(product);
    }

    // ── Delete (soft delete: an khoi ca admin va client) ──────────
    // BUG FIX: truoc day dat Status = 1 (draft), phai la Status = 3 (deleted)
    // Admin query: Status != 3 → Status = 3 se an khoi admin
    // Client query: Status == 2 → da khong hien, nhung de nhat quan dung Status = 3
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query()
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product not found");

        product.Status = 3;
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);
        await uow.SaveAsync(cancellationToken);
    }

    // ── Client: danh sach san pham voi day du filter ───────────────
    public async Task<PagedResult<ProductListItemDto>> GetProductListAsync(
        CancellationToken cancellationToken,
        int page,
        int pageSize,
        string? categorySlug = null,
        string? keyword = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null)
    {
        page = page <= 0 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = uow.Products.Query()
            .Where(p => p.Status == 2)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .AsQueryable();

        // MỚI - lấy cả category cha + con
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            var cat = await uow.Categories.Query()
                .FirstOrDefaultAsync(c => c.Slug == categorySlug, cancellationToken);

            if (cat != null)
            {
                var childIds = await uow.Categories.Query()
                    .Where(c => c.ParentId == cat.CategoryId)
                    .Select(c => c.CategoryId)
                    .ToListAsync(cancellationToken);

                var allIds = new HashSet<Guid>(childIds) { cat.CategoryId };
                query = query.Where(p => allIds.Contains(p.CategoryId));
            }
        }

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{keyword.Trim()}%"));

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        // Filter gia theo gia ban thuc te (SalePrice neu co, nguoc lai RegularPrice)
        if (minPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.RegularPrice) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.RegularPrice) <= maxPrice.Value);

        // Sort
        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice ?? p.RegularPrice),
            "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.RegularPrice),
            "name_asc" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.ProductId,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.SalePrice ?? p.RegularPrice,
                RegularPrice = p.RegularPrice,
                SalePrice = p.SalePrice,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                CategoryId = p.CategoryId,
                BrandName = p.Brand.Name,
                BrandId = p.BrandId,
                StockQuantity = p.StockQuantity,
                WarrantyMonths = p.WarrantyMonths,
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