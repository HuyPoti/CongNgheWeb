using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class FlashSaleServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IActivityLogService> _mockActivityLog;
    private readonly FlashSaleService _service;

    public FlashSaleServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _mockActivityLog = new Mock<IActivityLogService>();

        _mockMapper.Setup(m => m.Map<FlashSaleDto>(It.IsAny<FlashSale>()))
            .Returns((FlashSale fs) => new FlashSaleDto
            {
                FlashSaleId = fs.FlashSaleId,
                Title = fs.Title,
                IsActive = fs.IsActive
            });
        _mockMapper.Setup(m => m.Map<List<FlashSaleDto>>(It.IsAny<List<FlashSale>>()))
            .Returns((List<FlashSale> list) => list.Select(fs => new FlashSaleDto
            {
                FlashSaleId = fs.FlashSaleId,
                Title = fs.Title,
                IsActive = fs.IsActive
            }).ToList());
        _mockMapper.Setup(m => m.Map<FlashSaleItemDto>(It.IsAny<FlashSaleItem>()))
            .Returns((FlashSaleItem i) => new FlashSaleItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                FlashPrice = i.FlashPrice
            });

        _mockActivityLog.Setup(a => a.LogAsync(It.IsAny<CreateActivityLogDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActivityLogDto());

        _service = new FlashSaleService(_context, _mockMapper.Object, _mockActivityLog.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsBadRequest()
    {
        var dto = new CreateFlashSaleDto { Title = "", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*title*");
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsBadRequest()
    {
        var dto = new CreateFlashSaleDto { Title = "Test", StartTime = DateTime.UtcNow.AddHours(2), EndTime = DateTime.UtcNow };
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*EndTime*");
    }

    [Fact]
    public async Task CreateAsync_OverlappingActive_ThrowsBadRequest()
    {
        _context.FlashSales.Add(new FlashSale
        {
            FlashSaleId = Guid.NewGuid(), Title = "Existing",
            StartTime = DateTime.UtcNow.AddHours(-1), EndTime = DateTime.UtcNow.AddHours(5),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var dto = new CreateFlashSaleDto
        {
            Title = "New", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(3), IsActive = true
        };
        var act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already*active*");
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesFlashSale()
    {
        var dto = new CreateFlashSaleDto
        {
            Title = "Flash Sale", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2), IsActive = true
        };
        var result = await _service.CreateAsync(dto);
        result.Should().NotBeNull();
        result.Title.Should().Be("Flash Sale");
    }

    // ============================================================
    // GetActiveAsync
    // ============================================================

    [Fact]
    public async Task GetActiveAsync_NoActive_ReturnsNull()
    {
        var result = await _service.GetActiveAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAsync_HasActive_ReturnsIt()
    {
        _context.FlashSales.Add(new FlashSale
        {
            FlashSaleId = Guid.NewGuid(), Title = "Active",
            StartTime = DateTime.UtcNow.AddHours(-1), EndTime = DateTime.UtcNow.AddHours(5),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveAsync();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Active");
    }

    // ============================================================
    // GetFlashPriceAsync
    // ============================================================

    [Fact]
    public async Task GetFlashPriceAsync_NoActiveFlashSale_ReturnsNull()
    {
        var result = await _service.GetFlashPriceAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFlashPriceAsync_HasActiveItem_ReturnsPrice()
    {
        var productId = Guid.NewGuid();
        var flashSale = new FlashSale
        {
            FlashSaleId = Guid.NewGuid(), Title = "FS",
            StartTime = DateTime.UtcNow.AddHours(-1), EndTime = DateTime.UtcNow.AddHours(5),
            IsActive = true
        };
        _context.FlashSales.Add(flashSale);
        _context.FlashSaleItems.Add(new FlashSaleItem
        {
            Id = Guid.NewGuid(), FlashSaleId = flashSale.FlashSaleId,
            ProductId = productId, FlashPrice = 99, StockLimit = 10, SoldCount = 0
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetFlashPriceAsync(productId);
        result.Should().Be(99);
    }

    // ============================================================
    // AddItemAsync
    // ============================================================

    [Fact]
    public async Task AddItemAsync_FlashSaleNotFound_ThrowsNotFound()
    {
        var dto = new CreateFlashSaleItemDto { ProductId = Guid.NewGuid(), FlashPrice = 50, StockLimit = 10 };
        var act = () => _service.AddItemAsync(Guid.NewGuid(), dto);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Flash sale*");
    }

    [Fact]
    public async Task AddItemAsync_ProductNotFound_ThrowsNotFound()
    {
        var fs = new FlashSale { FlashSaleId = Guid.NewGuid(), Title = "FS", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2), IsActive = true };
        _context.FlashSales.Add(fs);
        await _context.SaveChangesAsync();

        var dto = new CreateFlashSaleItemDto { ProductId = Guid.NewGuid(), FlashPrice = 50, StockLimit = 10 };
        var act = () => _service.AddItemAsync(fs.FlashSaleId, dto);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Product*");
    }

    [Fact]
    public async Task AddItemAsync_ZeroStockLimit_ThrowsBadRequest()
    {
        var fs = new FlashSale { FlashSaleId = Guid.NewGuid(), Title = "FS", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
        var product = new Product
        {
            ProductId = Guid.NewGuid(), Name = "P", Slug = "p", Sku = "SKU1",
            RegularPrice = 200, SalePrice = 150, CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid()
        };
        _context.FlashSales.Add(fs);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var dto = new CreateFlashSaleItemDto { ProductId = product.ProductId, FlashPrice = 100, StockLimit = 0 };
        var act = () => _service.AddItemAsync(fs.FlashSaleId, dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*StockLimit*");
    }

    // ============================================================
    // RemoveItemAsync
    // ============================================================

    [Fact]
    public async Task RemoveItemAsync_NotFound_ThrowsNotFound()
    {
        var act = () => _service.RemoveItemAsync(Guid.NewGuid(), Guid.NewGuid(), null);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveItemAsync_Found_RemovesItem()
    {
        var productId = Guid.NewGuid();
        var fsId = Guid.NewGuid();
        _context.FlashSaleItems.Add(new FlashSaleItem
        {
            Id = Guid.NewGuid(), FlashSaleId = fsId, ProductId = productId,
            FlashPrice = 50, StockLimit = 10, SoldCount = 0
        });
        await _context.SaveChangesAsync();

        await _service.RemoveItemAsync(fsId, productId, null);
        var remaining = await _context.FlashSaleItems.CountAsync();
        remaining.Should().Be(0);
    }

    // ============================================================
    // GetAllAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        for (int i = 0; i < 5; i++)
        {
            _context.FlashSales.Add(new FlashSale
            {
                FlashSaleId = Guid.NewGuid(), Title = $"FS{i}",
                StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(1, 3);
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
    }
}
