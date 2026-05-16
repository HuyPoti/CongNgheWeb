using backend.DTOs;

namespace backend.Services;

public interface IReturnRequestService
{
    Task<IEnumerable<ReturnRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto> CreateAsync(Guid userId, CreateReturnRequestDto dto, CancellationToken cancellationToken = default);
    Task<ReturnRequestDto> ProcessAsync(Guid adminId, Guid returnId, UpdateReturnRequestDto dto, CancellationToken cancellationToken = default);
}
