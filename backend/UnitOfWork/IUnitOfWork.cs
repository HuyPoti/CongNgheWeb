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

    
    Task<int> SaveAsync(CancellationToken cancellationToken);
}
