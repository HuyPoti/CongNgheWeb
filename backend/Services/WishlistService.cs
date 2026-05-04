using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public WishlistService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WishlistItemDto>> GetByUserAsync(Guid userId)
    {
        var wishlists = await _context.Wishlists
            .Include(w => w.Product)
                .ThenInclude(p => p!.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<WishlistItemDto>>(wishlists);
    }

    public async Task<bool> ToggleAsync(Guid userId, Guid productId)
    {
        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing != null)
        {
            _context.Wishlists.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // Removed
        }

        // Check limit
        var count = await _context.Wishlists.CountAsync(w => w.UserId == userId);
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

        _context.Wishlists.Add(newItem);
        await _context.SaveChangesAsync();
        return true; // Added
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<int> CountAsync(Guid userId)
    {
        return await _context.Wishlists
            .CountAsync(w => w.UserId == userId);
    }
}
