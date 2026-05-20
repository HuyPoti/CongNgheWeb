using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend.Constants;
using Microsoft.AspNetCore.Http;

namespace backend.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly PaymentConfig _paymentConfig;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IVnPayService _vnPayService;
    private readonly AutoMapper.IMapper _mapper;

    public PaymentService(IUnitOfWork uow, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IVnPayService vnPayService, AutoMapper.IMapper mapper)
    {
        _uow = uow;
        _paymentConfig = configuration.GetSection("Payment").Get<PaymentConfig>() ?? new PaymentConfig();
        _httpContextAccessor = httpContextAccessor;
        _vnPayService = vnPayService;
        _mapper = mapper;
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

        // PaymentStatus.Completed = 2
        if (existingPayment != null && existingPayment.Status == PaymentStatus.Completed)
        {
            throw new BadRequestException("Order has already been paid");
        }

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Amount = order.TotalAmount,
            PaymentMethod = dto.PaymentMethod.ToLower(),
            Status = PaymentStatus.Pending,  // 1 = pending
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

            // Sinh mã QR chuyển khoản VietQR động (Sử dụng API img.vietqr.io chuẩn)
            var amountInt = (long)order.TotalAmount;
            var encodedOwner = Uri.EscapeDataString(_paymentConfig.BankOwner);
            var encodedInfo = Uri.EscapeDataString(order.OrderCode);
            response.QrUrl = $"https://img.vietqr.io/image/{_paymentConfig.BankId}-{_paymentConfig.BankAccount}-compact2.jpg?amount={amountInt}&addInfo={encodedInfo}&accountName={encodedOwner}";
        }
        else if (payment.PaymentMethod == "vnpay")
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new BadRequestException("HTTP Context is not available");
            }

            var vnPayRequest = new VnPayRequestDto
            {
                OrderId = payment.PaymentId, // Sử dụng PaymentId làm TxnRef để VNPAY không bị trùng TxnRef
                Amount = order.TotalAmount,
                Description = $"Thanh toan don hang {order.OrderCode}"
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest, httpContext);
            response.PaymentUrl = paymentUrl;
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

        // PaymentStatus.Completed = 2
        if (payment.Status == PaymentStatus.Completed)
            throw new BadRequestException("Payment has already been confirmed");

        payment.Status = PaymentStatus.Completed;  // 2 = success
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId = $"BT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId, cancellationToken);
        if (order != null)
        {
            order.PaymentStatus = PaymentStatus.Completed;
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

        // PaymentStatus.Completed = 2
        if (payment == null || payment.Status == PaymentStatus.Completed)
            return;

        payment.Status = PaymentStatus.Completed;  // 2 = success
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.TransactionId = $"COD-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = await _uow.Orders.Query()
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (order != null)
        {
            order.PaymentStatus = PaymentStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.SaveAsync(cancellationToken);
    }

    public async Task<PagedResult<PaymentTransactionDto>> GetAllTransactionsAsync(
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _uow.Payments.Query()
            .Include(p => p.Order)
            .ThenInclude(o => o.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower().Trim();
            query = query.Where(p => 
                (p.TransactionId != null && p.TransactionId.ToLower().Contains(keyword)) ||
                p.Order.OrderCode.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<PaymentTransactionDto>>(payments);

        return new PagedResult<PaymentTransactionDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
