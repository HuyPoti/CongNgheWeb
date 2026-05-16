using backend.DTOs;

namespace backend.Services;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentDto> ConfirmBankTransferAsync(Guid paymentId, Guid confirmedByUserId, CancellationToken cancellationToken = default);
    Task CompleteCodPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
}
