using AutoMapper;
using backend.UnitOfWork;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public WishlistService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WishlistItemDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wishlists = await _uow.Wishlists.Query()
            .Include(w => w.Product)
                .ThenInclude(p => p!.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<WishlistItemDto>>(wishlists);
    }

    public async Task<bool> ToggleAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var existing = await _uow.Wishlists.Query()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (existing != null)
        {
            _uow.Wishlists.Delete(existing);
            await _uow.SaveAsync(cancellationToken);
            return false; // Removed
        }

        // Check limit
        var count = await _uow.Wishlists.Query().CountAsync(w => w.UserId == userId, cancellationToken);
        if (count >= 50)
        {
            throw new InvalidOperationException("Danh sách yêu thích đã đầy (tối đa 50 sản phẩm).");
        }

        var newItem = new Wishlist
        {
            WishlistId = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        _uow.Wishlists.Insert(newItem);
        await _uow.SaveAsync(cancellationToken);
        return true; // Added
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _uow.Wishlists.Query()
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
    }

    public async Task<int> CountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _uow.Wishlists.Query()
            .CountAsync(w => w.UserId == userId, cancellationToken);
    }
}
