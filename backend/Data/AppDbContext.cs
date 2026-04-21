using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductSpec> ProductSpecs { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<NewsCategory> NewsCategories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewImage> ReviewImages { get; set; }
    public DbSet<ReviewReply> ReviewReplies { get; set; }
    public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens {get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReviewHelpfulVote>(entity =>
        {
            entity.ToTable("review_helpful_votes");
            entity.HasKey(e => e.VoteId);
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            entity.HasOne(e => e.Review)
                .WithMany(r => r.HelpfulVotes)
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Banners
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.ToTable("banners");
            entity.HasKey(e => e.BannerId);
            entity.Property(e => e.Position).HasDefaultValue(BannerPosition.homepage_mid);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Brands
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.BrandId);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Categories
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.CategoryId);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Products
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.ProductId);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Sku).IsUnique();

            entity.Property(e => e.Specifications).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            entity.HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.BrandId);
        });

        // Product Images
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(e => e.ImageId);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId);
        });

        // Product Specs (Key-Value)
        modelBuilder.Entity<ProductSpec>(entity =>
        {
            entity.ToTable("product_specs");
            entity.HasKey(e => e.SpecId);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Specs)
                .HasForeignKey(e => e.ProductId);
        });

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasDefaultValue(UserRole.customer);
        });

        // News Categories
        modelBuilder.Entity<NewsCategory>(entity =>
        {
            entity.ToTable("news_categories");
            entity.HasKey(e => e.CategoryId);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // News
        modelBuilder.Entity<News>(entity =>
        {
            entity.ToTable("news");
            entity.HasKey(e => e.NewsId);
            entity.HasIndex(n => n.Slug).IsUnique();

            entity.HasOne(n => n.Category)
                .WithMany()
                .HasForeignKey(n => n.CategoryId);

            entity.HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId);
        });

        // Reviews
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(e => e.ReviewId);

            entity.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review Images
        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.ToTable("review_images");
            entity.HasKey(e => e.ImageId);
            entity.HasOne(e => e.Review)
                .WithMany(r => r.Images)
                .HasForeignKey(e => e.ReviewId);
        });

        // Review Replies
        modelBuilder.Entity<ReviewReply>(entity =>
        {
            entity.ToTable("review_replies");
            entity.HasKey(e => e.ReplyId);
            entity.HasOne(e => e.Review)
                .WithMany(r => r.Replies)
                .HasForeignKey(e => e.ReviewId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity => 
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Refresh Tokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
