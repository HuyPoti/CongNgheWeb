using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace backend.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IEmailNotificationService> _mockEmailNotification;
    private readonly OrderService _service;

    private readonly Mock<IRepository<Order>> _mockOrderRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<Product>> _mockProductRepo;
    private readonly Mock<IRepository<Address>> _mockAddressRepo;
    private readonly Mock<IRepository<OrderStatusHistory>> _mockHistoryRepo;

    public OrderServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockEmailNotification = new Mock<IEmailNotificationService>();

        _mockOrderRepo = new Mock<IRepository<Order>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockProductRepo = new Mock<IRepository<Product>>();
        _mockAddressRepo = new Mock<IRepository<Address>>();
        _mockHistoryRepo = new Mock<IRepository<OrderStatusHistory>>();

        _mockUow.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.Products).Returns(_mockProductRepo.Object);
        _mockUow.Setup(u => u.Addresses).Returns(_mockAddressRepo.Object);
        _mockUow.Setup(u => u.OrderStatusHistories).Returns(_mockHistoryRepo.Object);

        _mockEmailNotification
            .Setup(e => e.SendOrderConfirmedEmail(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockEmailNotification
            .Setup(e => e.SendOrderShippingEmail(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockEmailNotification
            .Setup(e => e.SendOrderDeliveredEmail(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _service = new OrderService(_mockUow.Object, _mockEmailNotification.Object);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsBadRequest()
    {
        var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto>() };
        var act = () => _service.CreateAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*at least one item*");
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ThrowsNotFound()
    {
        var userId = Guid.NewGuid();
        var users = new List<User>().AsQueryable().BuildMock();
        _mockUserRepo.Setup(r => r.Query()).Returns(users);

        var dto = new CreateOrderDto
        {
            UserId = userId,
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };
        var act = () => _service.CreateAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*User*");
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_OrderNotFound_ThrowsNotFound()
    {
        var orders = new List<Order>().AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var act = () => _service.UpdateAsync(Guid.NewGuid(), new UpdateOrderDto { Status = "confirmed" }, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Order not found*");
    }

    [Fact]
    public async Task UpdateAsync_ValidStatusChange_UpdatesOrderAndCreatesHistory()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId, UserId = Guid.NewGuid(), Status = 1,
            PaymentStatus = 1, TotalAmount = 100, OrderCode = "ORD-1"
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);
        _mockOrderRepo.Setup(r => r.Update(It.IsAny<Order>())).Returns(order);
        _mockHistoryRepo.Setup(r => r.Insert(It.IsAny<OrderStatusHistory>())).Returns(new OrderStatusHistory());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.UpdateAsync(orderId, new UpdateOrderDto { Status = "confirmed" }, CancellationToken.None);

        result.Should().BeTrue();
        order.Status.Should().Be(2);
        _mockHistoryRepo.Verify(r => r.Insert(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    // ============================================================
    // CancelAsync
    // ============================================================

    [Fact]
    public async Task CancelAsync_OrderNotFound_ThrowsNotFound()
    {
        var orders = new List<Order>().AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var act = () => _service.CancelAsync(Guid.NewGuid(), new CancelOrderDto { Reason = "test" }, null, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelAsync_AlreadyDelivered_ThrowsBadRequest()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = 5,
            TotalAmount = 100, OrderCode = "ORD-2",
            OrderItems = new List<OrderItem>()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var act = () => _service.CancelAsync(order.OrderId, new CancelOrderDto { Reason = "test" }, null, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*delivered or cancelled*");
    }

    [Fact]
    public async Task CancelAsync_ValidOrder_CancelsAndRestoresStock()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId, Name = "P", Slug = "p", Sku = "S1",
            StockQuantity = 5, RegularPrice = 100,
            CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid()
        };
        var order = new Order
        {
            OrderId = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = 1,
            TotalAmount = 100, OrderCode = "ORD-3",
            OrderItems = new List<OrderItem>
            {
                new() { OrderItemId = Guid.NewGuid(), ProductId = productId, Quantity = 2, UnitPrice = 100 }
            }
        };

        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var products = new List<Product> { product }.AsQueryable().BuildMock();
        _mockProductRepo.Setup(r => r.Query()).Returns(products);

        _mockOrderRepo.Setup(r => r.Update(It.IsAny<Order>())).Returns(order);
        _mockProductRepo.Setup(r => r.Update(It.IsAny<Product>())).Returns(product);
        _mockHistoryRepo.Setup(r => r.Insert(It.IsAny<OrderStatusHistory>())).Returns(new OrderStatusHistory());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CancelAsync(order.OrderId, new CancelOrderDto { Reason = "Test cancel" }, null, CancellationToken.None);

        result.Should().BeTrue();
        order.Status.Should().Be(6);
        product.StockQuantity.Should().Be(7); // 5 + 2 restored
    }

    // ============================================================
    // GetAllAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAll()
    {
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, FullName = "Customer 1" };
        var ordersList = new List<Order>
        {
            new() { OrderId = Guid.NewGuid(), OrderCode = "ORD-1", Status = 1, UserId = userId, User = user, CreatedAt = DateTime.UtcNow },
            new() { OrderId = Guid.NewGuid(), OrderCode = "ORD-2", Status = 2, UserId = userId, User = user, CreatedAt = DateTime.UtcNow.AddHours(-1) }
        };

        var mockOrders = ordersList.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(mockOrders);

        var result = await _service.GetAllAsync(null, null, 1, 10, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.First().OrderCode.Should().Be("ORD-1");
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsOrderDetail()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, FullName = "Customer 1" };
        var address = new Address { AddressId = Guid.NewGuid(), UserId = userId, RecipientName = "R", Phone = "123", AddressLine = "A" };
        var product = new Product { ProductId = Guid.NewGuid(), Name = "P1" };
        
        var order = new Order
        {
            OrderId = orderId,
            OrderCode = "ORD-1",
            UserId = userId,
            User = user,
            Address = address,
            Status = 1,
            PaymentStatus = 1,
            OrderItems = new List<OrderItem>
            {
                new() { OrderItemId = Guid.NewGuid(), ProductId = product.ProductId, Product = product, Quantity = 1, UnitPrice = 100 }
            }
        };

        var mockOrders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(mockOrders);

        var result = await _service.GetByIdAsync(orderId, userId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrderCode.Should().Be("ORD-1");
        result.CustomerName.Should().Be("Customer 1");
        result.Items.Should().HaveCount(1);
    }

    // ============================================================
    // GetStatusHistoryAsync
    // ============================================================

    [Fact]
    public async Task GetStatusHistoryAsync_Found_ReturnsHistory()
    {
        var orderId = Guid.NewGuid();
        var historyList = new List<OrderStatusHistory>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId, NewStatus = 1, Note = "Note 1", CreatedAt = DateTime.UtcNow }
        };

        var mockHistory = historyList.AsQueryable().BuildMock();
        _mockHistoryRepo.Setup(r => r.Query()).Returns(mockHistory);

        var result = await _service.GetStatusHistoryAsync(orderId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Note.Should().Be("Note 1");
    }
}
