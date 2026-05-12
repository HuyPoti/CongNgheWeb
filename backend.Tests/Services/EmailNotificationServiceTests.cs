using Moq;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Services;

public class EmailNotificationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly EmailNotificationService _service;

    public EmailNotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockEmailService = new Mock<IEmailService>();
        _service = new EmailNotificationService(_mockEmailService.Object, _context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Order> CreateTestOrder(Guid orderId, string orderCode, string userEmail)
    {
        var user = new User { UserId = Guid.NewGuid(), FullName = "Test User", Email = userEmail };
        var order = new Order { OrderId = orderId, OrderCode = orderCode, UserId = user.UserId, User = user };
        _context.Users.Add(user);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task SendOrderConfirmedEmail_ValidOrder_SendsEmail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var email = "test@example.com";
        await CreateTestOrder(orderId, "ORD123", email);

        // Act
        await _service.SendOrderConfirmedEmail(orderId);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(
            email, 
            It.Is<string>(s => s.Contains("ORD123")), 
            It.Is<string>(b => b.Contains("xác nhận"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendOrderShippingEmail_ValidOrder_SendsEmail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var email = "test@example.com";
        await CreateTestOrder(orderId, "ORD123", email);

        // Act
        await _service.SendOrderShippingEmail(orderId);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(
            email, 
            It.Is<string>(s => s.Contains("ORD123")), 
            It.Is<string>(b => b.Contains("vận chuyển"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendOrderDeliveredEmail_ValidOrder_SendsEmail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var email = "test@example.com";
        await CreateTestOrder(orderId, "ORD123", email);

        // Act
        await _service.SendOrderDeliveredEmail(orderId);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(
            email, 
            It.Is<string>(s => s.Contains("ORD123") && s.Contains("giao thành công")), 
            It.Is<string>(b => b.Contains("Cảm ơn"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendReturnProcessedEmail_ApprovedRequest_SendsEmail()
    {
        // Arrange
        var returnId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid(), FullName = "User", Email = "user@test.com" };
        var order = new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-RET" };
        var request = new ReturnRequest 
        { 
            ReturnId = returnId, 
            UserId = user.UserId, 
            User = user, 
            OrderId = order.OrderId, 
            Order = order,
            Status = "approved",
            AdminNote = "Fine"
        };
        _context.Users.Add(user);
        _context.Orders.Add(order);
        _context.ReturnRequests.Add(request);
        await _context.SaveChangesAsync();

        // Act
        await _service.SendReturnProcessedEmail(returnId);

        // Assert
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "user@test.com", 
            It.Is<string>(s => s.Contains("ORD-RET")), 
            It.Is<string>(b => b.Contains("CHẤP NHẬN"))), 
            Times.Once);
    }
}
