using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using backend.MapperProfiles;
using backend.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.Services;

public class ProductImageServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductImageService _service;
    private readonly IMapper _mapper;

    public ProductImageServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { 
            cfg.AddMaps(typeof(ProductProfile).Assembly); 
            cfg.AddMaps(typeof(ProductImageProfile).Assembly);
        });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new ProductImageService(uow, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByProductIdAsync_ProductNotFound_ThrowsNotFound()
    {
        // Act
        var act = () => _service.GetByProductIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByProductIdAsync_ValidProduct_ReturnsImages()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = catId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Slug = "p", Sku = "S", RegularPrice = 100, CategoryId = catId, BrandId = brandId });
        _context.ProductImages.Add(new ProductImage { ImageId = Guid.NewGuid(), ProductId = productId, ImageUrl = "url1", SortOrder = 1 });
        _context.ProductImages.Add(new ProductImage { ImageId = Guid.NewGuid(), ProductId = productId, ImageUrl = "url2", SortOrder = 2 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByProductIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().ImageUrl.Should().Be("url1");
    }

    [Fact]
    public async Task AddAsync_ValidInput_AddsImage()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = catId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Slug = "p", Sku = "S", RegularPrice = 100, CategoryId = catId, BrandId = brandId });
        await _context.SaveChangesAsync();
        var dto = new CreateProductImageDto { ImageUrl = "new-url", IsPrimary = true, SortOrder = 0 };

        // Act
        var result = await _service.AddAsync(productId, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ImageUrl.Should().Be("new-url");
        _context.ProductImages.Count(x => x.ProductId == productId).Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_NewPrimary_ResetsOldPrimary()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = catId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Slug = "p", Sku = "S", RegularPrice = 100, CategoryId = catId, BrandId = brandId });
        _context.ProductImages.Add(new ProductImage { ImageId = Guid.NewGuid(), ProductId = productId, ImageUrl = "old-primary", IsPrimary = true });
        await _context.SaveChangesAsync();
        var dto = new CreateProductImageDto { ImageUrl = "new-primary", IsPrimary = true };

        // Act
        await _service.AddAsync(productId, dto, CancellationToken.None);

        // Assert
        var oldPrimary = await _context.ProductImages.FirstOrDefaultAsync(x => x.ImageUrl == "old-primary");
        oldPrimary!.IsPrimary.Should().BeFalse();
        var newPrimary = await _context.ProductImages.FirstOrDefaultAsync(x => x.ImageUrl == "new-primary");
        newPrimary!.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ExistingImage_DeletesIt()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        _context.ProductImages.Add(new ProductImage { ImageId = imageId, ProductId = Guid.NewGuid(), ImageUrl = "url" });
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(imageId, CancellationToken.None);

        // Assert
        _context.ProductImages.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFound()
    {
        // Act
        var act = () => _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
