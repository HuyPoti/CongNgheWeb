using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace backend.Tests.Services;

public class ShipmentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly ShipmentService _service;
    private readonly Mock<IRepository<Shipment>> _mockShipmentRepo;
    private readonly Mock<IRepository<Order>> _mockOrderRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<OrderStatusHistory>> _mockHistoryRepo;

    public ShipmentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockShipmentRepo = new Mock<IRepository<Shipment>>();
        _mockOrderRepo = new Mock<IRepository<Order>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockHistoryRepo = new Mock<IRepository<OrderStatusHistory>>();

        _mockUow.Setup(u => u.Shipments).Returns(_mockShipmentRepo.Object);
        _mockUow.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.OrderStatusHistories).Returns(_mockHistoryRepo.Object);

        _service = new ShipmentService(_mockUow.Object);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_OrderNotFound_ThrowsNotFound()
    {
        var orders = new List<Order>().AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var dto = new CreateShipmentDto { OrderId = Guid.NewGuid(), Carrier = "GHN" };
        var act = () => _service.CreateAsync(dto, Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_OrderNotConfirmed_ThrowsBadRequest()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(), Status = 1, TotalAmount = 100,
            OrderCode = "ORD-1", UserId = Guid.NewGuid()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var dto = new CreateShipmentDto { OrderId = order.OrderId, Carrier = "GHN" };
        var act = () => _service.CreateAsync(dto, Guid.NewGuid());
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*confirmed*");
    }

    [Fact]
    public async Task CreateAsync_ShipmentAlreadyExists_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId, Status = 2, TotalAmount = 100,
            OrderCode = "ORD-2", UserId = Guid.NewGuid()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var shipments = new List<Shipment>
        {
            new() { ShipmentId = Guid.NewGuid(), OrderId = orderId }
        }.AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        var dto = new CreateShipmentDto { OrderId = orderId, Carrier = "GHN" };
        var act = () => _service.CreateAsync(dto, Guid.NewGuid());
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateAsync_Valid_CreatesShipmentAndMovesToProcessing()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId, Status = 2, TotalAmount = 100,
            OrderCode = "ORD-3", UserId = Guid.NewGuid()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var shipments = new List<Shipment>().AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        _mockShipmentRepo.Setup(r => r.Insert(It.IsAny<Shipment>())).Returns(new Shipment());
        _mockOrderRepo.Setup(r => r.Update(It.IsAny<Order>())).Returns(order);
        _mockHistoryRepo.Setup(r => r.Insert(It.IsAny<OrderStatusHistory>())).Returns(new OrderStatusHistory());

        // For MapToDto helper - user lookup for PackedBy
        var users = new List<User>().AsQueryable().BuildMock();
        _mockUserRepo.Setup(r => r.Query()).Returns(users);

        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateShipmentDto { OrderId = orderId, Carrier = "GHN" };
        var result = await _service.CreateAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        order.Status.Should().Be(3); // Processing
        _mockShipmentRepo.Verify(r => r.Insert(It.IsAny<Shipment>()), Times.Once);
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFound()
    {
        var shipments = new List<Shipment>().AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        var act = () => _service.UpdateAsync(Guid.NewGuid(), new UpdateShipmentDto());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ============================================================
    // GetByOrderIdAsync
    // ============================================================

    [Fact]
    public async Task GetByOrderIdAsync_NotFound_ReturnsNull()
    {
        var shipments = new List<Shipment>().AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        var result = await _service.GetByOrderIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    // ============================================================
    // MarkQcPassedAsync
    // ============================================================

    [Fact]
    public async Task MarkQcPassedAsync_NotFound_ThrowsNotFound()
    {
        var shipments = new List<Shipment>().AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        var act = () => _service.MarkQcPassedAsync(Guid.NewGuid(), new MarkQcDto(), Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MarkQcPassedAsync_Valid_SetsQcPassed()
    {
        var shipment = new Shipment
        {
            ShipmentId = Guid.NewGuid(), OrderId = Guid.NewGuid(), QcPassed = false, Status = "packing"
        };
        var shipments = new List<Shipment> { shipment }.AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);
        _mockShipmentRepo.Setup(r => r.Update(It.IsAny<Shipment>())).Returns(shipment);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var users = new List<User>().AsQueryable().BuildMock();
        _mockUserRepo.Setup(r => r.Query()).Returns(users);

        var result = await _service.MarkQcPassedAsync(shipment.ShipmentId, new MarkQcDto { QcPassed = true }, Guid.NewGuid());

        result.Should().NotBeNull();
        shipment.QcPassed.Should().BeTrue();
        shipment.Status.Should().Be("qc_passed");
    }

    // ============================================================
    // MarkPackedAsync
    // ============================================================

    [Fact]
    public async Task MarkPackedAsync_NotFound_ThrowsNotFound()
    {
        var shipments = new List<Shipment>().AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);

        var act = () => _service.MarkPackedAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MarkPackedAsync_Valid_SetsPackedStatus()
    {
        var userId = Guid.NewGuid();
        var shipment = new Shipment
        {
            ShipmentId = Guid.NewGuid(), OrderId = Guid.NewGuid(), Status = "qc_passed"
        };
        var shipments = new List<Shipment> { shipment }.AsQueryable().BuildMock();
        _mockShipmentRepo.Setup(r => r.Query()).Returns(shipments);
        _mockShipmentRepo.Setup(r => r.Update(It.IsAny<Shipment>())).Returns(shipment);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var user = new User { UserId = userId, FullName = "Packer", Email = "p@p.com" };
        var users = new List<User> { user }.AsQueryable().BuildMock();
        _mockUserRepo.Setup(r => r.Query()).Returns(users);

        var result = await _service.MarkPackedAsync(shipment.ShipmentId, userId);

        result.Should().NotBeNull();
        shipment.Status.Should().Be("packed");
        shipment.PackedBy.Should().Be(userId);
        result.PackedByName.Should().Be("Packer");
    }
}
