using backend.DTOs;

namespace backend.Services;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ToggleAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid userId, CancellationToken cancellationToken = default);
}
