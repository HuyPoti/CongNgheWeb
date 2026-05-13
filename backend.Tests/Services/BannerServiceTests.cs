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

public class BannerServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BannerService _service;
    private readonly IMapper _mapper;

    public BannerServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(BannerProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new BannerService(uow, _mapper);
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
    public async Task GetAllAsync_ReturnsAllBanners()
    {
        // Arrange
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), Title = "B1", ImageUrl = "img1" });
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), Title = "B2", ImageUrl = "img2" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // ============================================================
    // GetPublicAsync
    // ============================================================

    [Fact]
    public async Task GetPublicAsync_ValidBanners_ReturnsActiveAndInDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), IsActive = true, StartDate = today.AddDays(-1), EndDate = today.AddDays(1), Title = "Valid", ImageUrl = "img" });
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), IsActive = true, StartDate = today.AddDays(1), Title = "Future", ImageUrl = "img" });
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), IsActive = true, EndDate = today.AddDays(-1), Title = "Expired", ImageUrl = "img" });
        _context.Banners.Add(new Banner { BannerId = Guid.NewGuid(), IsActive = false, Title = "Inactive", ImageUrl = "img" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPublicAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Valid");
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsBanner()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Banners.Add(new Banner { BannerId = id, Title = "Found", ImageUrl = "img" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Found");
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_InvalidDates_ReturnsNull()
    {
        // Arrange
        var dto = new CreateBannerDto 
        { 
            Title = "Title", 
            ImageUrl = "img", 
            StartDate = DateTime.UtcNow.AddDays(1), 
            EndDate = DateTime.UtcNow.AddDays(-1) 
        };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedBanner()
    {
        // Arrange
        var dto = new CreateBannerDto { Title = "New", ImageUrl = "img" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        
        _context.Banners.Count().Should().Be(1);
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateBannerDto(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesAndReturnsBanner()
    {
        // Arrange
        var id = Guid.NewGuid();
        _context.Banners.Add(new Banner { BannerId = id, Title = "Old", ImageUrl = "img" });
        await _context.SaveChangesAsync();

        var dto = new UpdateBannerDto { Title = "Updated" };

        // Act
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
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
        _context.Banners.Add(new Banner { BannerId = id, IsActive = true, Title = "To Delete", ImageUrl = "img" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        var banner = await _context.Banners.FindAsync(id);
        banner!.IsActive.Should().BeFalse();
    }
}
