using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using backend.MapperProfiles;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace backend.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IProductService _service;
    private readonly Mock<IProductImageService> _imageServiceMock;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(UserProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        _imageServiceMock = new Mock<IProductImageService>();
        var uow = new backend.UnitOfWork.UnitOfWork(_context, mapper);
        _service = new ProductService(uow, mapper, _imageServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsProduct()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = categoryId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        _context.Products.Add(new Product { ProductId = productId, Name = "Test Product", Slug = "test-prod", Sku = "SKU1", Status = 2, CategoryId = categoryId, BrandId = brandId });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(productId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAll()
    {
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = categoryId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        _context.Products.Add(new Product { ProductId = Guid.NewGuid(), Name = "P1", Slug = "p1", Sku = "S1", Status = 2, RegularPrice = 100, CategoryId = categoryId, BrandId = brandId });
        _context.Products.Add(new Product { ProductId = Guid.NewGuid(), Name = "P2", Slug = "p2", Sku = "S2", Status = 2, RegularPrice = 200, CategoryId = categoryId, BrandId = brandId });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(null, null, null, null, CancellationToken.None, 1, 10);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesProduct()
    {
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = categoryId, Name = "Cat", Slug = "cat" });
        _context.Brands.Add(new Brand { BrandId = brandId, Name = "Brand", Slug = "brand" });
        await _context.SaveChangesAsync();

        var dto = new CreateProductDto
        {
            Name = "New Product",
            Slug = "new-product",
            RegularPrice = 1000,
            CategoryId = categoryId,
            BrandId = brandId,
            Status = 2
        };

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        _context.Products.Should().Contain(p => p.Name == "New Product");
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_MarksAsDeleted()
    {
        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "To Delete", Slug = "del", Sku = "D1", Status = 2, CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(productId, CancellationToken.None);

        var product = await _context.Products.FindAsync(productId);
        product.Should().NotBeNull();
        product!.Status.Should().Be(3); // 3: deleted
    }
}
