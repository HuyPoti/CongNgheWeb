using AutoMapper;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.UnitOfWork;

public class UnitOfWork(AppDbContext dbContext, IMapper mapper) : IUnitOfWork
{
    public IRepository<Product> Products { get; } = new Repository<Product>(dbContext, mapper);
    public IRepository<Category> Categories { get; } = new Repository<Category>(dbContext, mapper);
    public IRepository<Brand> Brands { get; } = new Repository<Brand>(dbContext, mapper);
    public IRepository<Banner> Banners { get; } = new Repository<Banner>(dbContext, mapper);
    public IRepository<User> Users { get; } = new Repository<User>(dbContext, mapper);
    public IRepository<ProductImage> ProductImages { get; } = new Repository<ProductImage>(dbContext, mapper);
    public IRepository<News> News { get; } = new Repository<News>(dbContext, mapper);
    public IRepository<NewsCategory> NewsCategories { get; } = new Repository<NewsCategory>(dbContext, mapper);
    public IRepository<Order> Orders { get; } = new Repository<Order>(dbContext, mapper);
    public IRepository<Address> Addresses { get; } = new Repository<Address>(dbContext, mapper);
    public IRepository<Review> Reviews { get; } = new Repository<Review>(dbContext, mapper);
    public IRepository<ReviewImage> ReviewImages { get; } = new Repository<ReviewImage>(dbContext, mapper);
    public IRepository<ReviewReply> ReviewReplies { get; } = new Repository<ReviewReply>(dbContext, mapper);
    public IRepository<ReviewHelpfulVote> ReviewHelpfulVotes { get; } = new Repository<ReviewHelpfulVote>(dbContext, mapper);
    public IRepository<Payment> Payments { get; } = new Repository<Payment>(dbContext, mapper);

    public IRepository<Supplier> Suppliers { get; } = new Repository<Supplier>(dbContext, mapper);
    public IRepository<InventoryReceipt> InventoryReceipts { get; } = new Repository<InventoryReceipt>(dbContext, mapper);
    public IRepository<InventoryReceiptItem> InventoryReceiptItems { get; } = new Repository<InventoryReceiptItem>(dbContext, mapper);
    public IRepository<InventoryTransaction> InventoryTransactions { get; } = new Repository<InventoryTransaction>(dbContext, mapper);

    public IRepository<Shipment> Shipments { get; } = new Repository<Shipment>(dbContext, mapper);
    public IRepository<OrderStatusHistory> OrderStatusHistories { get; } = new Repository<OrderStatusHistory>(dbContext, mapper);
    public IRepository<Wishlist> Wishlists { get; } = new Repository<Wishlist>(dbContext, mapper);
    public IRepository<ReturnRequest> ReturnRequests { get; } = new Repository<ReturnRequest>(dbContext, mapper);
    public IRepository<ReturnRequestItem> ReturnRequestItems { get; } = new Repository<ReturnRequestItem>(dbContext, mapper);
    public IRepository<ReturnRequestImage> ReturnRequestImages { get; } = new Repository<ReturnRequestImage>(dbContext, mapper);
    public IRepository<OrderItem> OrderItems { get; } = new Repository<OrderItem>(dbContext, mapper);
    public IRepository<Coupon> Coupons { get; } = new Repository<Coupon>(dbContext, mapper);
    public IRepository<CouponUsage> CouponUsages { get; } = new Repository<CouponUsage>(dbContext, mapper);
    public IRepository<FlashSale> FlashSales { get; } = new Repository<FlashSale>(dbContext, mapper);
    public IRepository<FlashSaleItem> FlashSaleItems { get; } = new Repository<FlashSaleItem>(dbContext, mapper);
    public IRepository<ActivityLog> ActivityLogs { get; } = new Repository<ActivityLog>(dbContext, mapper);
    public IRepository<CartItem> CartItems { get; } = new Repository<CartItem>(dbContext, mapper);
    public IRepository<PasswordResetToken> PasswordResetTokens { get; } = new Repository<PasswordResetToken>(dbContext, mapper);
    public IRepository<RefreshToken> RefreshTokens { get; } = new Repository<RefreshToken>(dbContext, mapper);

    public async Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
