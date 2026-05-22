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
        var now = DateTime.UtcNow;

        var dbQuery = uow.Products.Query()
            .Where(p => p.Status == ProductStatus.Published)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.FlashSaleItems)
            .ThenInclude(f => f.FlashSale)
            .AsNoTracking();

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

        // Lấy tất cả products trước khi filter theo giá
        var allProducts = await dbQuery.ToListAsync(cancellationToken);

        // Tính giá hiệu lực (FlashPrice nếu có, không SalePrice)
        var productsWithEffectivePrice = allProducts.Select(p => new
        {
            Product = p,
            EffectivePrice = GetEffectivePrice(p, now)
        }).ToList();

        // Filter theo giá hiệu lực
        if (query.MinPrice.HasValue)
            productsWithEffectivePrice = productsWithEffectivePrice
                .Where(x => x.EffectivePrice >= query.MinPrice.Value)
                .ToList();

        if (query.MaxPrice.HasValue)
            productsWithEffectivePrice = productsWithEffectivePrice
                .Where(x => x.EffectivePrice <= query.MaxPrice.Value)
                .ToList();

        // Sort theo giá hiệu lực
        productsWithEffectivePrice = query.SortBy switch
        {
            "price_asc" => productsWithEffectivePrice.OrderBy(x => x.EffectivePrice).ToList(),
            "price_desc" => productsWithEffectivePrice.OrderByDescending(x => x.EffectivePrice).ToList(),
            "name_asc" => productsWithEffectivePrice.OrderBy(x => x.Product.Name).ToList(),
            _ => productsWithEffectivePrice.OrderByDescending(x => x.Product.CreatedAt).ToList()
        };

        var totalCount = productsWithEffectivePrice.Count;

        // Pagination
        var pagedProducts = productsWithEffectivePrice
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        // Chuyển sang DTO
        var items = pagedProducts.Select(x => new ProductListItemDto
        {
            Id = x.Product.ProductId,
            Name = x.Product.Name,
            Slug = x.Product.Slug,
            Price = x.EffectivePrice,
            RegularPrice = x.Product.RegularPrice,
            SalePrice = GetFlashPrice(x.Product, now) ?? x.Product.SalePrice,
            CategoryName = x.Product.Category.Name,
            CategorySlug = x.Product.Category.Slug,
            CategoryId = x.Product.CategoryId,
            BrandName = x.Product.Brand.Name,
            BrandId = x.Product.BrandId,
            StockQuantity = x.Product.StockQuantity,
            WarrantyMonths = x.Product.WarrantyMonths,
            IsFlashSale = GetFlashPrice(x.Product, now).HasValue,
            ThumbnailUrl = x.Product.Images
                .Where(i => i.IsPrimary)
                .OrderBy(i => i.SortOrder)
                .Select(i => i.ImageUrl)
                .FirstOrDefault()
                ?? x.Product.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
        }).ToList();

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    /// <summary>
    /// Lấy giá flash sale (nếu có flash sale đang hoạt động), ngược lại trả về null
    /// </summary>
    private decimal? GetFlashPrice(Product product, DateTime now)
    {
        var nowUtc = NormalizeDateTime(now);
        
        var activeFlashItem = product.FlashSaleItems
            .Where(f => f.FlashSale != null
                && f.FlashSale.IsActive
                && NormalizeDateTime(f.FlashSale.StartTime) <= nowUtc
                && NormalizeDateTime(f.FlashSale.EndTime) >= nowUtc
                && f.SoldCount < f.StockLimit)
            .OrderBy(f => f.FlashPrice)
            .FirstOrDefault();

        return activeFlashItem?.FlashPrice;
    }

    /// <summary>
    /// Normalize DateTime to UTC, handling DateTimeKind.Unspecified from database
    /// </summary>
    private DateTime NormalizeDateTime(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc) // Unspecified - assume UTC
        };
    }

    /// <summary>
    /// Lấy giá hiệu lực (FlashPrice nếu có flash sale, SalePrice nếu có, RegularPrice)
    /// </summary>
    private decimal GetEffectivePrice(Product product, DateTime now)
    {
        var flashPrice = GetFlashPrice(product, now);
        if (flashPrice.HasValue)
            return flashPrice.Value;

        return product.SalePrice ?? product.RegularPrice;
    }
}