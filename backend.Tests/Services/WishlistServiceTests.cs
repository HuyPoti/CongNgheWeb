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

public class WishlistServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IWishlistService _service;

    public WishlistServiceTests()
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

        var uow = new backend.UnitOfWork.UnitOfWork(_context, mapper);
        _service = new WishlistService(uow, mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsUserWishlistItems()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "User 1", Email = "u1@a.com", IsActive = true, PasswordHash = "h" });
        _context.Products.Add(new Product { ProductId = productId, Name = "Prod 1", Slug = "p1", Sku = "SKU1" });
        _context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetByUserAsync(userId);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ToggleAsync_NewItem_AddsToWishlist()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var result = await _service.ToggleAsync(userId, productId);

        result.Should().BeTrue(); // Added
        _context.Wishlists.Should().HaveCount(1);
    }

    [Fact]
    public async Task ToggleAsync_ExistingItem_RemovesFromWishlist()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = productId });
        await _context.SaveChangesAsync();

        var result = await _service.ToggleAsync(userId, productId);

        result.Should().BeFalse(); // Removed
        _context.Wishlists.Should().BeEmpty();
    }

    [Fact]
    public async Task IsInWishlistAsync_Exists_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = productId });
        await _context.SaveChangesAsync();

        var result = await _service.IsInWishlistAsync(userId, productId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var userId = Guid.NewGuid();
        _context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = Guid.NewGuid() });
        _context.Wishlists.Add(new Wishlist { WishlistId = Guid.NewGuid(), UserId = userId, ProductId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var result = await _service.CountAsync(userId);

        result.Should().Be(2);
    }
}
