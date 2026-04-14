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
    IRepository<ProductSpec> ProductSpecs { get; }
    IRepository<News> News { get; }
    IRepository<NewsCategory> NewsCategories { get; }
    IRepository<Order> Orders { get; }
    Task<int> SaveAsync(CancellationToken cancellationToken);
}
