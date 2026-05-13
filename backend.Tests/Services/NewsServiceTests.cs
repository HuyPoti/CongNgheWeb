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

public class NewsServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly NewsService _service;
    private readonly IMapper _mapper;

    public NewsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { 
            cfg.AddMaps(typeof(NewsProfile).Assembly); 
            cfg.AddMaps(typeof(NewsCategoryProfile).Assembly);
            cfg.AddMaps(typeof(UserProfile).Assembly);
        });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new NewsService(uow, _mapper);
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
        var catId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        _context.NewsCategories.Add(new NewsCategory { CategoryId = catId, Name = "Cat1", Slug = "cat1" });
        _context.Users.Add(new User { UserId = authorId, FullName = "Author1", Email = "a1@test.com", PasswordHash = "x" });
        
        _context.News.Add(new News { NewsId = Guid.NewGuid(), Title = "N1", Slug = "n1", Content = "C1", CategoryId = catId, AuthorId = authorId });
        _context.News.Add(new News { NewsId = Guid.NewGuid(), Title = "N2", Slug = "n2", Content = "C2", CategoryId = catId, AuthorId = authorId });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().CategoryName.Should().Be("Cat1");
        result.First().AuthorName.Should().Be("Author1");
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsNews()
    {
        // Arrange
        var id = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        _context.NewsCategories.Add(new NewsCategory { CategoryId = catId, Name = "Cat1", Slug = "cat1" });
        _context.Users.Add(new User { UserId = authorId, FullName = "Author1", Email = "a1@test.com", PasswordHash = "x" });
        _context.News.Add(new News { NewsId = id, Title = "Found", Slug = "found", Content = "C", CategoryId = catId, AuthorId = authorId });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.NewsId.Should().Be(id);
        result.Title.Should().Be("Found");
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreated()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var dto = new CreateNewsDto { Title = "New", Slug = "new", Content = "Content", CategoryId = catId, AuthorId = authorId };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New");
        _context.News.Count().Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAndReturns()
    {
        // Arrange
        var id = Guid.NewGuid();
        var catId = Guid.NewGuid();
        _context.News.Add(new News { NewsId = id, Title = "Old", Slug = "old", Content = "C", CategoryId = catId, AuthorId = Guid.NewGuid() });
        await _context.SaveChangesAsync();
        var dto = new UpdateNewsDto { Title = "Updated" };

        // Act
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        var entity = await _context.News.FindAsync(id);
        entity!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_Found_MarksInactive()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.News.Add(new News { NewsId = id, IsActive = true, Title = "X", Slug = "x", Content = "C", CategoryId = Guid.NewGuid(), AuthorId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var entity = await _context.News.FindAsync(id);
        entity!.IsActive.Should().BeFalse();
    }
}
