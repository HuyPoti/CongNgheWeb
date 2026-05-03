using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

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

    // Thông tin tài khoản ngân hàng
    private const string BANK_NAME = "Vietcombank";
    private const string BANK_ACCOUNT = "1234567890";
    private const string BANK_OWNER = "CONG TY TNHH GEARVN";

    public PaymentService(IUnitOfWork uow)
    {
        _uow = uow;
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

        if (existingPayment != null && existingPayment.Status == "success")
        {
            throw new BadRequestException("Order has already been paid");
        }

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Amount = order.TotalAmount,
            PaymentMethod = dto.PaymentMethod.ToLower(),
            Status = "pending",
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
            Status = payment.Status
        };

        if (payment.PaymentMethod == "bank_transfer")
        {
            response.BankInfo = $"Ngân hàng: {BANK_NAME}\n" +
                               $"Số tài khoản: {BANK_ACCOUNT}\n" +
                               $"Chủ tài khoản: {BANK_OWNER}\n" +
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

        if (payment.Status == "success")
            throw new BadRequestException("Payment has already been confirmed");

        payment.Status = "success";
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

        if (payment == null || payment.Status == "success")
            return;

        payment.Status = "success";
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
