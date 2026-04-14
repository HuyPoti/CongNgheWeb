using backend.DTOs;

namespace backend.Services;

public interface IOrderService
{
    // GET: Order list with pagination + filter
    Task<PagedResult<OrderDto>> GetAllAsync(
        string? status, // Filter by status
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );

    // GET: A order detail
    Task<OrderDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    );

    // PUT: Update status/payment_status
    Task<bool> UpdateAsync(
        Guid id,
        UpdateOrderDto dto,
        CancellationToken cancellationToken
    );
}
