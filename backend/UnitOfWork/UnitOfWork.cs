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

    public async Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
