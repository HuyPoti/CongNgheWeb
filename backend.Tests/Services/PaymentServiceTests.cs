using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace backend.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly PaymentService _service;
    private readonly Mock<IRepository<Payment>> _mockPaymentRepo;
    private readonly Mock<IRepository<Order>> _mockOrderRepo;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IVnPayService> _mockVnPayService;
    private readonly Mock<IMapper> _mockMapper;

    public PaymentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockPaymentRepo = new Mock<IRepository<Payment>>();
        _mockOrderRepo = new Mock<IRepository<Order>>();
        _mockUow.Setup(u => u.Payments).Returns(_mockPaymentRepo.Object);
        _mockUow.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockVnPayService = new Mock<IVnPayService>();
        _mockMapper = new Mock<IMapper>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Payment:BankName", "Vietcombank" },
                { "Payment:BankAccount", "1234567890" },
                { "Payment:BankOwner", "NGUYEN VAN A" }
            })
            .Build();

        _service = new PaymentService(_mockUow.Object, config, _mockHttpContextAccessor.Object, _mockVnPayService.Object, _mockMapper.Object);
    }

    // ============================================================
    // CreatePaymentAsync
    // ============================================================

    [Fact]
    public async Task CreatePaymentAsync_OrderNotFound_ThrowsNotFound()
    {
        var orders = new List<Order>().AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var act = () => _service.CreatePaymentAsync(new CreatePaymentDto { OrderId = Guid.NewGuid(), PaymentMethod = "cod" });
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreatePaymentAsync_AlreadyPaid_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 100, OrderCode = "ORD-1", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>
        {
            new() { PaymentId = Guid.NewGuid(), OrderId = orderId, Status = 2, CreatedAt = DateTime.UtcNow }
        }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var act = () => _service.CreatePaymentAsync(new CreatePaymentDto { OrderId = orderId, PaymentMethod = "cod" });
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already been paid*");
    }

    [Fact]
    public async Task CreatePaymentAsync_BankTransfer_ReturnsBankInfo()
    {
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 100, OrderCode = "ORD-BT", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId, PaymentMethod = "bank_transfer"
        });

        result.Should().NotBeNull();
        result.BankInfo.Should().Contain("Vietcombank");
    }

    [Fact]
    public async Task CreatePaymentAsync_Cod_NoBankInfo()
    {
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 100, OrderCode = "ORD-COD", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId, PaymentMethod = "cod"
        });

        result.BankInfo.Should().BeNull();
    }

    // ============================================================
    // GetByOrderIdAsync
    // ============================================================

    [Fact]
    public async Task GetByOrderIdAsync_NotFound_ReturnsNull()
    {
        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var result = await _service.GetByOrderIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_Found_ReturnsPayment()
    {
        var orderId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new()
            {
                PaymentId = Guid.NewGuid(), OrderId = orderId, Amount = 100,
                PaymentMethod = "cod", Status = 1, CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var result = await _service.GetByOrderIdAsync(orderId);
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
    }

    // ============================================================
    // ConfirmBankTransferAsync
    // ============================================================

    [Fact]
    public async Task ConfirmBankTransferAsync_NotFound_ThrowsNotFound()
    {
        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var act = () => _service.ConfirmBankTransferAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ConfirmBankTransferAsync_NotBankTransfer_ThrowsBadRequest()
    {
        var paymentId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new() { PaymentId = paymentId, PaymentMethod = "cod", Status = 1, OrderId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var act = () => _service.ConfirmBankTransferAsync(paymentId, Guid.NewGuid());
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*bank transfer*");
    }

    [Fact]
    public async Task ConfirmBankTransferAsync_AlreadyConfirmed_ThrowsBadRequest()
    {
        var paymentId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new() { PaymentId = paymentId, PaymentMethod = "bank_transfer", Status = 2, OrderId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var act = () => _service.ConfirmBankTransferAsync(paymentId, Guid.NewGuid());
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*already been confirmed*");
    }

    [Fact]
    public async Task ConfirmBankTransferAsync_Valid_ConfirmsPayment()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var payment = new Payment
        {
            PaymentId = paymentId, PaymentMethod = "bank_transfer", Status = 1, OrderId = orderId
        };
        var payments = new List<Payment> { payment }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        var order = new Order { OrderId = orderId, PaymentStatus = 1, TotalAmount = 100, OrderCode = "ORD-X", UserId = Guid.NewGuid() };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.ConfirmBankTransferAsync(paymentId, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Status.Should().Be(2);
        order.PaymentStatus.Should().Be(2);
    }

    // ============================================================
    // CompleteCodPaymentAsync
    // ============================================================

    [Fact]
    public async Task CompleteCodPaymentAsync_NoPayment_DoesNotThrow()
    {
        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);

        await _service.CompleteCodPaymentAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task CreatePaymentAsync_VnPay_WithHttpContext_ReturnsPaymentUrl()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 500000, OrderCode = "ORD-VNPAY", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var httpContext = new Mock<HttpContext>();
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext.Object);
        _mockVnPayService.Setup(v => v.CreatePaymentUrl(It.IsAny<VnPayRequestDto>(), It.IsAny<HttpContext>()))
            .Returns("https://sandbox.vnpayment.vn/paygate/...");

        // Act
        var result = await _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId,
            PaymentMethod = "vnpay",
            ReturnUrl = "http://localhost/return"
        });

        // Assert
        result.Should().NotBeNull();
        result.PaymentUrl.Should().NotBeNullOrEmpty();
        result.PaymentMethod.Should().Be("vnpay");
    }

    [Fact]
    public async Task CreatePaymentAsync_VnPay_NoHttpContext_ThrowsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 500000, OrderCode = "ORD-VNPAY2", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        // Act
        var act = () => _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId,
            PaymentMethod = "vnpay",
            ReturnUrl = "http://localhost/return"
        });

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*HTTP Context*");
    }

    [Fact]
    public async Task CreatePaymentAsync_BankTransfer_GeneratesQRCode()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 1000000, OrderCode = "ORD-BT-QR", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId,
            PaymentMethod = "bank_transfer"
        });

        // Assert
        result.Should().NotBeNull();
        result.QrUrl.Should().Contain("vietqr.io");
        result.QrUrl.Should().Contain("1000000"); // Amount in QR
    }

    [Fact]
    public async Task CreatePaymentAsync_CaseInsensitivePaymentMethod_Works()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 100, OrderCode = "ORD-CASE", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payments = new List<Payment>().AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreatePaymentAsync(new CreatePaymentDto
        {
            OrderId = orderId,
            PaymentMethod = "COD"  // uppercase
        });

        // Assert
        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be("cod"); // should be normalized to lowercase
    }

    [Fact]
    public async Task CreatePaymentAsync_MultiplePaymentsForOrder_ReturnLatestOne()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { OrderId = orderId, TotalAmount = 200, OrderCode = "ORD-MULTI", UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        var payment1 = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            Status = 1,
            PaymentMethod = "cod",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var payment2 = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            Status = 1,
            PaymentMethod = "bank_transfer",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var payments = new List<Payment> { payment1, payment2 }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Insert(It.IsAny<Payment>())).Returns(new Payment());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(payment2.PaymentId); // Latest should be returned
    }

    [Fact]
    public async Task ConfirmBankTransferAsync_Valid_UpdatesOrderPaymentStatus()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var payment = new Payment
        {
            PaymentId = paymentId,
            PaymentMethod = "bank_transfer",
            Status = 1,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };
        var payments = new List<Payment> { payment }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Update(It.IsAny<Payment>())).Returns(payment);

        var order = new Order
        {
            OrderId = orderId,
            PaymentStatus = 1,
            Status = 2,
            TotalAmount = 1500000,
            OrderCode = "ORD-CONFIRM-BT",
            UserId = Guid.NewGuid()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);
        _mockOrderRepo.Setup(r => r.Update(It.IsAny<Order>())).Returns(order);

        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ConfirmBankTransferAsync(paymentId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(2); // Completed
        payment.PaidAt.Should().NotBeNull();
        order.PaymentStatus.Should().Be(2);
    }

    [Fact]
    public async Task CompleteCodPaymentAsync_MulitplePaymentsForOrder_CompletesLatest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment1 = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethod = "cod",
            Status = 2, // Already completed
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var payment2 = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethod = "cod",
            Status = 1, // Pending
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var payments = new List<Payment> { payment1, payment2 }.AsQueryable().BuildMock();
        _mockPaymentRepo.Setup(r => r.Query()).Returns(payments);
        _mockPaymentRepo.Setup(r => r.Update(It.IsAny<Payment>())).Returns(payment2);

        var order = new Order
        {
            OrderId = orderId,
            PaymentStatus = 1,
            TotalAmount = 300000,
            OrderCode = "ORD-COD-MULTI",
            UserId = Guid.NewGuid()
        };
        var orders = new List<Order> { order }.AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);
        _mockOrderRepo.Setup(r => r.Update(It.IsAny<Order>())).Returns(order);

        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.CompleteCodPaymentAsync(orderId);

        // Assert
        payment2.Status.Should().Be(2);
        order.PaymentStatus.Should().Be(2);
    }
}
