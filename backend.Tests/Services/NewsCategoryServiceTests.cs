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

public class NewsCategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NewsCategoryService _service;
    private readonly IMapper _mapper;

    public NewsCategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(NewsCategoryProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new NewsCategoryService(uow, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        // Arrange
        _context.NewsCategories.Add(new NewsCategory { CategoryId = Guid.NewGuid(), Name = "NC1", Slug = "nc1" });
        _context.NewsCategories.Add(new NewsCategory { CategoryId = Guid.NewGuid(), Name = "NC2", Slug = "nc2" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.NewsCategories.Add(new NewsCategory { CategoryId = id, Name = "Found", Slug = "found" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(id);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateNewsCategoryDto { Name = "New", Slug = "new" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New");
        _context.NewsCategories.Count().Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAndReturns()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.NewsCategories.Add(new NewsCategory { CategoryId = id, Name = "Old", Slug = "old" });
        await _context.SaveChangesAsync();
        var dto = new UpdateNewsCategoryDto { Name = "Updated" };

        // Act
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_Found_HardDeletes()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.NewsCategories.Add(new NewsCategory { CategoryId = id, IsActive = true, Name = "X", Slug = "x" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var entity = await _context.NewsCategories.FindAsync(id);
        entity.Should().BeNull();
    }
}
