using backend.DTOs;

namespace backend.Services;

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemDto>> GetByUserAsync(Guid userId);
    Task<bool> ToggleAsync(Guid userId, Guid productId);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId);
    Task<int> CountAsync(Guid userId);
}
