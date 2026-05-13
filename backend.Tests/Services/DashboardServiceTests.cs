using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Services;

public class DashboardServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new DashboardService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // GetOverviewAsync
    // ============================================================

    [Fact]
    public async Task GetOverviewAsync_ReturnsCorrectStatistics()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Revenue (Status != 6, PaymentStatus == 2)
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 100, Status = 1, PaymentStatus = 2, CreatedAt = now });
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 50, Status = 6, PaymentStatus = 2, CreatedAt = now }); // Cancelled
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 200, Status = 1, PaymentStatus = 1, CreatedAt = now }); // Unpaid

        // TotalOrders (Created >= startOfMonth)
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), CreatedAt = startOfMonth.AddDays(1) });
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), CreatedAt = startOfMonth.AddDays(-1) }); // Old month

        // Customers
        _context.Users.Add(new User { UserId = Guid.NewGuid(), Role = UserRole.customer, Email = "c1@test.com" });
        _context.Users.Add(new User { UserId = Guid.NewGuid(), Role = UserRole.admin, Email = "a1@test.com" });

        // Coupons
        _context.Coupons.Add(new Coupon { Code = "C1", IsActive = true, StartDate = now.AddDays(-1), EndDate = now.AddDays(1) });
        _context.Coupons.Add(new Coupon { Code = "C2", IsActive = false, StartDate = now.AddDays(-1), EndDate = now.AddDays(1) });

        // FlashSales
        _context.FlashSales.Add(new FlashSale { FlashSaleId = Guid.NewGuid(), IsActive = true, StartTime = now.AddDays(-1), EndTime = now.AddDays(1) });

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOverviewAsync(CancellationToken.None);

        // Assert
        result.TotalRevenue.Should().Be(100);
        result.TotalOrders.Should().Be(4); // 3 (revenue group) + 1 (startOfMonth) - 0 (actually 100+50+200 + 2 = 5? No, wait)
        // Let's re-calculate:
        // Revenue orders: O1 (valid), O2 (cancelled), O3 (unpaid) -> 3 orders
        // Month orders: O4 (valid month), O5 (old month) -> 2 orders
        // Total Orders >= startOfMonth: O1, O2, O3, O4 -> 4 orders. Correct.
        result.TotalCustomers.Should().Be(1);
        result.ActiveCoupons.Should().Be(1);
        result.ActiveFlashSales.Should().Be(1);
    }

    // ============================================================
    // GetRevenueChartAsync
    // ============================================================

    [Fact]
    public async Task GetRevenueChartAsync_ReturnsGroupedRevenue()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 100, PaymentStatus = 2, CreatedAt = today });
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 50, PaymentStatus = 2, CreatedAt = today });
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), TotalAmount = 200, PaymentStatus = 2, CreatedAt = today.AddDays(-1) });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRevenueChartAsync(7, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First(x => x.Date == today).Revenue.Should().Be(150);
        result.First(x => x.Date == today).OrderCount.Should().Be(2);
        result.First(x => x.Date == today.AddDays(-1)).Revenue.Should().Be(200);
    }

    // ============================================================
    // GetTopProductsAsync
    // ============================================================

    [Fact]
    public async Task GetTopProductsAsync_ReturnsSortedProducts()
    {
        // Arrange
        var p1 = new Product { ProductId = Guid.NewGuid(), Name = "P1", Slug = "p1" };
        var p2 = new Product { ProductId = Guid.NewGuid(), Name = "P2", Slug = "p2" };
        _context.Products.AddRange(p1, p2);

        var o1 = new Order { OrderId = Guid.NewGuid(), Status = 1 };
        _context.Orders.Add(o1);

        _context.OrderItems.Add(new OrderItem { OrderItemId = Guid.NewGuid(), OrderId = o1.OrderId, ProductId = p1.ProductId, Quantity = 5, UnitPrice = 10 });
        _context.OrderItems.Add(new OrderItem { OrderItemId = Guid.NewGuid(), OrderId = o1.OrderId, ProductId = p2.ProductId, Quantity = 10, UnitPrice = 5 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTopProductsAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].ProductName.Should().Be("P2"); // 10 units
        result[0].UnitsSold.Should().Be(10);
        result[1].ProductName.Should().Be("P1"); // 5 units
    }

    // ============================================================
    // GetTopCustomersAsync
    // ============================================================

    [Fact]
    public async Task GetTopCustomersAsync_ReturnsSortedCustomers()
    {
        // Arrange
        var u1 = new User { UserId = Guid.NewGuid(), FullName = "User 1", Email = "u1@test.com" };
        var u2 = new User { UserId = Guid.NewGuid(), FullName = "User 2", Email = "u2@test.com" };
        _context.Users.AddRange(u1, u2);

        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), UserId = u1.UserId, TotalAmount = 500, PaymentStatus = 2, Status = 1 });
        _context.Orders.Add(new Order { OrderId = Guid.NewGuid(), UserId = u2.UserId, TotalAmount = 1000, PaymentStatus = 2, Status = 1 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTopCustomersAsync(5, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("User 2"); // 1000 spent
        result[0].TotalSpent.Should().Be(1000);
    }
}
