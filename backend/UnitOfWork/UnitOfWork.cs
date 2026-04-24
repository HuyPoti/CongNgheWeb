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

    public async Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
