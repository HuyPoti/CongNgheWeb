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

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICategoryService _service;
    private readonly IMapper _mapper;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(CategoryProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new CategoryService(uow, _mapper);
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
    public async Task GetAllAsync_ReturnsActiveCategories()
    {
        // Arrange
        _context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), Name = "C1", Slug = "c1", IsActive = true });
        _context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), Name = "C2", Slug = "c2", IsActive = false });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("C1");
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = id, Name = "Found", Slug = "found" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(id);
    }

    // ============================================================
    // GetBySlugAsync
    // ============================================================

    [Fact]
    public async Task GetBySlugAsync_Found_ReturnsCategory()
    {
        // Arrange
        var slug = "test-slug";
        _context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), Name = "Test", Slug = slug });
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
        _context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), Name = "Existing", Slug = "duplicate" });
        await _context.SaveChangesAsync();

        var dto = new CreateCategoryDto { Name = "New", Slug = "duplicate" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Cat X", Slug = "cat-x" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cat X");
        _context.Categories.Count().Should().Be(1);
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateCategoryDto(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAndReturnsCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Categories.Add(new Category { CategoryId = id, Name = "Old", Slug = "old" });
        await _context.SaveChangesAsync();

        var dto = new UpdateCategoryDto { Name = "New Name" };

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
        _context.Categories.Add(new Category { CategoryId = id, IsActive = true, Name = "To Delete", Slug = "del" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        var category = await _context.Categories.FindAsync(id);
        category!.IsActive.Should().BeFalse();
    }
}
