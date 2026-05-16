using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using backend.Extensions;
using backend.Constants;


namespace backend.Services;

public class ProductService(
    IUnitOfWork uow,
    IMapper mapper,
    IProductImageService imageService) : IProductService
{
    // ── Admin: danh sach co filter ─────────────────────────────────
    public async Task<PagedResult<ProductDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var dbQuery = uow.Products.Query()
            .Where(p => p.Status != ProductStatus.Deleted)
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            dbQuery = kw.Length <= 3
                ? dbQuery.Where(p => EF.Functions.ILike(p.Name, $"{kw}%"))
                : dbQuery.Where(p => EF.Functions.ILike(p.Name, $"%{kw}%"));
        }

        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.MinPrice.HasValue)
            dbQuery = dbQuery.Where(p => p.RegularPrice >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            dbQuery = dbQuery.Where(p => p.RegularPrice <= query.MaxPrice.Value);

        return await dbQuery
            .OrderByDescending(p => p.CreatedAt)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
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
            .Where(p => p.Status != ProductStatus.Deleted)
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
            .Where(p => p.Status != ProductStatus.Deleted)
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
        product.Status = dto.Status ?? ProductStatus.Draft;

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

        product.Status = ProductStatus.Deleted;
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);
        await uow.SaveAsync(cancellationToken);
    }

    // ── Client: danh sach san pham voi day du filter ───────────────
    public async Task<PagedResult<ProductListItemDto>> GetProductListAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken)
    {
        var dbQuery = uow.Products.Query()
            .Where(p => p.Status == ProductStatus.Published)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .AsQueryable();

        // MỚI - lấy cả category cha + con
        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            var cat = await uow.Categories.Query()
                .FirstOrDefaultAsync(c => c.Slug == query.CategorySlug, cancellationToken);

            if (cat != null)
            {
                var childIds = await uow.Categories.Query()
                    .Where(c => c.ParentId == cat.CategoryId)
                    .Select(c => c.CategoryId)
                    .ToListAsync(cancellationToken);

                var allIds = new HashSet<Guid>(childIds) { cat.CategoryId };
                dbQuery = dbQuery.Where(p => allIds.Contains(p.CategoryId));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            dbQuery = dbQuery.Where(p => EF.Functions.ILike(p.Name, $"%{query.Keyword.Trim()}%"));

        if (query.BrandId.HasValue)
            dbQuery = dbQuery.Where(p => p.BrandId == query.BrandId.Value);

        // Filter gia theo gia ban thuc te (SalePrice neu co, nguoc lai RegularPrice)
        if (query.MinPrice.HasValue)
            dbQuery = dbQuery.Where(p => (p.SalePrice ?? p.RegularPrice) >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            dbQuery = dbQuery.Where(p => (p.SalePrice ?? p.RegularPrice) <= query.MaxPrice.Value);

        // Sort
        dbQuery = query.SortBy switch
        {
            "price_asc" => dbQuery.OrderBy(p => p.SalePrice ?? p.RegularPrice),
            "price_desc" => dbQuery.OrderByDescending(p => p.SalePrice ?? p.RegularPrice),
            "name_asc" => dbQuery.OrderBy(p => p.Name),
            _ => dbQuery.OrderByDescending(p => p.CreatedAt)
        };

        return await dbQuery
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
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}