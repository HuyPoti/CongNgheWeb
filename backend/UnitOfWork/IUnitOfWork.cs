using backend.Models;

namespace backend.UnitOfWork;

public interface IUnitOfWork
{
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Brand> Brands { get; }
    IRepository<Banner> Banners { get; }
    IRepository<User> Users { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<News> News { get; }
    IRepository<NewsCategory> NewsCategories { get; }
    IRepository<Order> Orders { get; }
    IRepository<Address> Addresses { get; }

    IRepository<Review> Reviews { get; }
    IRepository<ReviewImage> ReviewImages { get; }
    IRepository<ReviewReply> ReviewReplies { get; }
    IRepository<ReviewHelpfulVote> ReviewHelpfulVotes { get; }
    IRepository<Payment> Payments { get; }

    IRepository<Supplier> Suppliers { get; }
    IRepository<InventoryReceipt> InventoryReceipts { get; }
    IRepository<InventoryReceiptItem> InventoryReceiptItems { get; }
    IRepository<InventoryTransaction> InventoryTransactions { get; }
    
    IRepository<Shipment> Shipments { get; }
    IRepository<OrderStatusHistory> OrderStatusHistories { get; }
    IRepository<Wishlist> Wishlists { get; }
    IRepository<ReturnRequest> ReturnRequests { get; }
    IRepository<ReturnRequestItem> ReturnRequestItems { get; }
    IRepository<ReturnRequestImage> ReturnRequestImages { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Coupon> Coupons { get; }
    IRepository<CouponUsage> CouponUsages { get; }
    IRepository<FlashSale> FlashSales { get; }
    IRepository<FlashSaleItem> FlashSaleItems { get; }
    IRepository<ActivityLog> ActivityLogs { get; }
    IRepository<CartItem> CartItems { get; }
    IRepository<PasswordResetToken> PasswordResetTokens { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    
    Task<int> SaveAsync(CancellationToken cancellationToken);
}
