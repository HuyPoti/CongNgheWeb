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

namespace backend.Tests.Services;

public class BrandServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IBrandService _service;
    private readonly IMapper _mapper;

    public BrandServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(BrandProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new BrandService(uow, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // GetAllAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsAllBrands()
    {
        // Arrange
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "B1", Slug = "b1" });
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "B2", Slug = "b2" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsBrand()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Brands.Add(new Brand { BrandId = id, Name = "B1", Slug = "b1" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.BrandId.Should().Be(id);
    }

    // ============================================================
    // GetBySlugAsync
    // ============================================================

    [Fact]
    public async Task GetBySlugAsync_Found_ReturnsBrand()
    {
        // Arrange
        var slug = "test-slug";
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "Test", Slug = slug });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBySlugAsync(slug, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be(slug);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_DuplicateSlug_ReturnsNull()
    {
        // Arrange
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "Existing", Slug = "duplicate" });
        await _context.SaveChangesAsync();

        var dto = new CreateBrandDto { Name = "New", Slug = "duplicate" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedBrand()
    {
        // Arrange
        var dto = new CreateBrandDto { Name = "Brand X", Slug = "brand-x" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Brand X");
        _context.Brands.Count().Should().Be(1);
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateBrandDto(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DuplicateSlug_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Brands.Add(new Brand { BrandId = id, Name = "B1", Slug = "s1" });
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "B2", Slug = "s2" });
        await _context.SaveChangesAsync();

        var dto = new UpdateBrandDto { Slug = "s2" }; // Update s1 to s2 (already exists)

        // Act
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAndReturnsBrand()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Brands.Add(new Brand { BrandId = id, Name = "Old", Slug = "old" });
        await _context.SaveChangesAsync();

        var dto = new UpdateBrandDto { Name = "New Name" };

        // Act
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Found_MarksInactiveAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Brands.Add(new Brand { BrandId = id, IsActive = true, Name = "To Delete" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        var brand = await _context.Brands.FindAsync(id);
        brand!.IsActive.Should().BeFalse();
    }
}
