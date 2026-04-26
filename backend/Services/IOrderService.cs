using backend.DTOs;

namespace backend.Services;

public interface IOrderService
{
    // POST: Create order from client cart items
    Task<OrderDetailDto> CreateAsync(
        CreateOrderDto dto,
        CancellationToken cancellationToken
    );

    // GET: Order list with pagination + filter
    Task<PagedResult<OrderDto>> GetAllAsync(
        string? status, // Filter by status
        Guid? userId,   // Filter by user
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );

    // GET: A order detail
    Task<OrderDetailDto?> GetByIdAsync(
        Guid id,
        Guid? userId,
        CancellationToken cancellationToken
    );

    // PUT: Update status/payment_status
    Task<bool> UpdateAsync(
        Guid id,
        UpdateOrderDto dto,
        CancellationToken cancellationToken
    );
}
