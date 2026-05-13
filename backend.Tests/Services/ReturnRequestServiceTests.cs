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

        _service = new ReturnRequestService(_context, mapper);
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
}
