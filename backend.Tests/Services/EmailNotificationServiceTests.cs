using Moq;
using AutoMapper;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Services;

public class EmailNotificationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly EmailNotificationService _service;
    private readonly IMapper _mapper;

    public EmailNotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _mockEmailService = new Mock<IEmailService>();

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(backend.MapperProfiles.CategoryProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new EmailNotificationService(_mockEmailService.Object, uow);
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

    // ============================================================
    // Email Notification - Null/Missing Data
    // ============================================================

    [Fact]
    public async Task SendOrderConfirmedEmail_OrderNotFound_DoesNotThrow()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act & Assert - should not throw
        await _service.SendOrderConfirmedEmail(nonExistentOrderId);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendOrderShippingEmail_OrderNotFound_DoesNotThrow()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act & Assert
        await _service.SendOrderShippingEmail(nonExistentOrderId);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendOrderDeliveredEmail_UserNull_DoesNotThrow()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = new Order { OrderId = orderId, OrderCode = "ORD-NULL", UserId = userId };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act & Assert
        await _service.SendOrderDeliveredEmail(orderId);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendReturnProcessedEmail_RequestNotFound_DoesNotThrow()
    {
        // Arrange
        var nonExistentReturnId = Guid.NewGuid();

        // Act & Assert
        await _service.SendReturnProcessedEmail(nonExistentReturnId);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendReturnProcessedEmail_RejectedRequest_IncludesRejectionStatus()
    {
        // Arrange
        var returnId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid(), FullName = "User", Email = "user@test.com" };
        var order = new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-REJ" };
        var request = new ReturnRequest 
        { 
            ReturnId = returnId, 
            UserId = user.UserId, 
            User = user, 
            OrderId = order.OrderId, 
            Order = order,
            Status = "rejected",
            AdminNote = "Does not meet criteria"
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
            It.Is<string>(s => s.Contains("ORD-REJ")), 
            It.Is<string>(b => b.Contains("TỪ CHỐI"))), 
            Times.Once);
    }

    [Fact]
    public async Task SendReturnProcessedEmail_CompletedRequest_IncludesCompletedStatus()
    {
        // Arrange
        var returnId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid(), FullName = "User", Email = "user@test.com" };
        var order = new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-COMP" };
        var request = new ReturnRequest 
        { 
            ReturnId = returnId, 
            UserId = user.UserId, 
            User = user, 
            OrderId = order.OrderId, 
            Order = order,
            Status = "completed",
            AdminNote = "Refund processed"
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
            It.Is<string>(s => s.Contains("ORD-COMP")), 
            It.Is<string>(b => b.Contains("HOÀN TẤT"))), 
            Times.Once);
    }
}
