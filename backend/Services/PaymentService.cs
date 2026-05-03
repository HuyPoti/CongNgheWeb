using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace backend.Services;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentDto> ConfirmBankTransferAsync(Guid paymentId, Guid confirmedByUserId, CancellationToken cancellationToken = default);
    Task CompleteCodPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly PaymentConfig _paymentConfig;

    public PaymentService(IUnitOfWork uow, IConfiguration configuration)
    {
        _uow = uow;
        _paymentConfig = configuration.GetSection("Payment").Get<PaymentConfig>() ?? new PaymentConfig();
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(
        CreatePaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order not found");

        var existingPayment = await _uow.Payments.Query()
            .Where(p => p.OrderId == dto.OrderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // 2 = success
        if (existingPayment != null && existingPayment.Status == 2)
        {
            throw new BadRequestException("Order has already been paid");
        }

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Amount = order.TotalAmount,
            PaymentMethod = dto.PaymentMethod.ToLower(),
            Status = 1,  // 1 = pending
            TransactionId = string.Empty,
            ReturnUrl = dto.ReturnUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _uow.Payments.Insert(payment);
        await _uow.SaveAsync(cancellationToken);

        var response = new CreatePaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status.ToString()
        };

        if (payment.PaymentMethod == "bank_transfer")
        {
            response.BankInfo = $"Ngân hàng: {_paymentConfig.BankName}\n" +
                               $"Số tài khoản: {_paymentConfig.BankAccount}\n" +
                               $"Chủ tài khoản: {_paymentConfig.BankOwner}\n" +
                               $"Nội dung chuyển khoản: {order.OrderCode}";
        }

        return response;
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _uow.Payments.Query()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null) return null;

        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            Status = payment.Status,
            GatewayResponse = payment.GatewayResponse,
            ReturnUrl = payment.ReturnUrl,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    public async Task<PaymentDto> ConfirmBankTransferAsync(
        Guid paymentId,
        Guid confirmedByUserId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _uow.Payments.Query()
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken);

        if (payment == null)
            throw new NotFoundException("Payment not found");

        if (payment.PaymentMethod != "bank_transfer")
            throw new BadRequestException("Only bank transfer payments can be confirmed this way");

        // 2 = success
        if (payment.Status == 2)
            throw new BadRequestException("Payment has already been confirmed");

        payment.Status = 2;  // 2 = success
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId = $"BT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId, cancellationToken);
        if (order != null)
        {
            order.PaymentStatus = 2;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveAsync(cancellationToken);

        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            Status = payment.Status,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    public async Task CompleteCodPaymentAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _uow.Payments.Query()
            .Where(p => p.OrderId == orderId && p.PaymentMethod == "cod")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // 2 = success
        if (payment == null || payment.Status == 2)
            return;

        payment.Status = 2;  // 2 = success
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId = $"COD-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (order != null)
        {
            order.PaymentStatus = 2;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveAsync(cancellationToken);
    }
}
