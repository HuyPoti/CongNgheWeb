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

public class ReturnRequestServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IReturnRequestService _service;
    private readonly Mock<IEmailNotificationService> _mockEmailNotification;

    public ReturnRequestServiceTests()
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
        _mockEmailNotification = new Mock<IEmailNotificationService>();
        _service = new ReturnRequestService(uow, mapper, _mockEmailNotification.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "User 1", Email = "u@u.com", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId1, UserId = userId });
        _context.Orders.Add(new Order { OrderId = orderId2, UserId = userId });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = Guid.NewGuid(), OrderId = orderId1, UserId = userId, Reason = "Reason 1" });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = Guid.NewGuid(), OrderId = orderId2, UserId = userId, Reason = "Reason 2" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = id, OrderId = orderId, UserId = userId, Reason = "Test" });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Reason.Should().Be("Test");
    }

    [Fact]
    public async Task GetByOrderIdAsync_Found_ReturnsDto()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = Guid.NewGuid(), OrderId = orderId, UserId = userId, Reason = "Test" });
        await _context.SaveChangesAsync();

        var result = await _service.GetByOrderIdAsync(orderId);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesRequest()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = DateTime.UtcNow }); // Status 5: Delivered
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Reason",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var result = await _service.CreateAsync(userId, dto);

        result.Should().NotBeNull();
        _context.ReturnRequests.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessAsync_Approve_UpdatesStatusAndRestoresStock()
    {
        var adminId = Guid.NewGuid();
        var returnId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var product = new Product { ProductId = productId, Name = "P", StockQuantity = 10, Slug = "p", Sku = "s" };
        _context.Products.Add(product);
        
        var request = new ReturnRequest
        {
            ReturnId = returnId,
            Status = "pending",
            Items = new List<ReturnRequestItem>
            {
                new() { Id = Guid.NewGuid(), OrderItemId = orderItemId, Quantity = 2, OrderItem = new OrderItem { OrderItemId = orderItemId, ProductId = productId } }
            }
        };
        _context.ReturnRequests.Add(request);
        await _context.SaveChangesAsync();

        var dto = new UpdateReturnRequestDto { Status = "approved", RefundAmount = 100 };

        var result = await _service.ProcessAsync(adminId, returnId, dto);

        result.Status.Should().Be("approved");
        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.StockQuantity.Should().Be(12); // 10 + 2 restored
    }

    // ============================================================
    // CreateAsync - Validation Tests
    // ============================================================

    [Fact]
    public async Task CreateAsync_OrderNotFound_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId, // Non-existent order
            Reason = "Reason",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var act = () => _service.CreateAsync(userId, dto);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*không tìm thấy đơn hàng*");
    }

    [Fact]
    public async Task CreateAsync_WrongUser_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U1", Email = "e1", PasswordHash = "h" });
        _context.Users.Add(new User { UserId = otherUserId, FullName = "U2", Email = "e2", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = otherUserId, Status = 5, UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Reason",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var act = () => _service.CreateAsync(userId, dto);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_OrderNotDelivered_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 4, UpdatedAt = DateTime.UtcNow }); // Status 4: Shipping, not 5: Delivered
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Reason",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var act = () => _service.CreateAsync(userId, dto);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*giao thành công*");
    }

    [Fact]
    public async Task CreateAsync_BeyondSevenDays_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var deliveredDate = DateTime.UtcNow.AddDays(-8); // 8 days ago

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = deliveredDate });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Reason",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var act = () => _service.CreateAsync(userId, dto);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*7 ngày*");
    }

    [Fact]
    public async Task CreateAsync_WithinSevenDays_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var deliveredDate = DateTime.UtcNow.AddDays(-6); // 6 days ago, still within limit

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = deliveredDate });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Defective product",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var result = await _service.CreateAsync(userId, dto);

        result.Should().NotBeNull();
        result.Reason.Should().Be("Defective product");
    }

    [Fact]
    public async Task CreateAsync_DuplicateReturn_ThrowsException()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = DateTime.UtcNow });
        _context.ReturnRequests.Add(new ReturnRequest
        {
            ReturnId = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Status = "pending",
            Reason = "First request"
        });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Second request",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = new List<string>()
        };

        var act = () => _service.CreateAsync(userId, dto);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*đã có yêu cầu đổi trả*");
    }

    [Fact]
    public async Task CreateAsync_WithItems_StoresItems()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = DateTime.UtcNow });
        _context.OrderItems.Add(new OrderItem { OrderItemId = orderItemId, OrderId = orderId, ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 });
        await _context.SaveChangesAsync();

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Items damaged",
            Items = new List<CreateReturnRequestItemDto>
            {
                new() { OrderItemId = orderItemId, Quantity = 1, ReasonDetail = "Scratched" }
            },
            ImageUrls = new List<string>()
        };

        var result = await _service.CreateAsync(userId, dto);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_WithImages_StoresImages()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        _context.Users.Add(new User { UserId = userId, FullName = "U", Email = "e", PasswordHash = "h" });
        _context.Orders.Add(new Order { OrderId = orderId, UserId = userId, Status = 5, UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var imageUrls = new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        };

        var dto = new CreateReturnRequestDto
        {
            OrderId = orderId,
            Reason = "Evidence needed",
            Items = new List<CreateReturnRequestItemDto>(),
            ImageUrls = imageUrls
        };

        var result = await _service.CreateAsync(userId, dto);

        result.Should().NotBeNull();
        result.Images.Should().HaveCount(2);
    }

    // ============================================================
    // ProcessAsync - Status Transitions
    // ============================================================

    [Fact]
    public async Task ProcessAsync_Reject_DoesNotRestoreStock()
    {
        var adminId = Guid.NewGuid();
        var returnId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var product = new Product { ProductId = productId, Name = "P", StockQuantity = 10, Slug = "p", Sku = "s" };
        _context.Products.Add(product);
        
        var request = new ReturnRequest
        {
            ReturnId = returnId,
            Status = "pending",
            Items = new List<ReturnRequestItem>
            {
                new() { Id = Guid.NewGuid(), OrderItemId = orderItemId, Quantity = 2, OrderItem = new OrderItem { OrderItemId = orderItemId, ProductId = productId } }
            }
        };
        _context.ReturnRequests.Add(request);
        await _context.SaveChangesAsync();

        var dto = new UpdateReturnRequestDto { Status = "rejected" };

        var result = await _service.ProcessAsync(adminId, returnId, dto);

        result.Status.Should().Be("rejected");
        var product_check = await _context.Products.FindAsync(productId);
        product_check!.StockQuantity.Should().Be(10); // Stock should NOT be restored
    }

    [Fact]
    public async Task ProcessAsync_UpdateRefundAmount_RecordsAmount()
    {
        var adminId = Guid.NewGuid();
        var returnId = Guid.NewGuid();

        var request = new ReturnRequest
        {
            ReturnId = returnId,
            Status = "pending",
            Items = new List<ReturnRequestItem>()
        };
        _context.ReturnRequests.Add(request);
        await _context.SaveChangesAsync();

        var refundAmount = 250000m;
        var dto = new UpdateReturnRequestDto { Status = "approved", RefundAmount = refundAmount };

        var result = await _service.ProcessAsync(adminId, returnId, dto);

        result.RefundAmount.Should().Be(refundAmount);
    }

    [Fact]
    public async Task ProcessAsync_NotFound_ThrowsException()
    {
        var adminId = Guid.NewGuid();
        var returnId = Guid.NewGuid();

        var act = () => _service.ProcessAsync(adminId, returnId, new UpdateReturnRequestDto { Status = "approved" });
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
