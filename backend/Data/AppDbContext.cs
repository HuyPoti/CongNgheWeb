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
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<ReturnRequestItem> ReturnRequestItems { get; set; }
    public DbSet<ReturnRequestImage> ReturnRequestImages { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<FlashSale> FlashSales { get; set; }
    public DbSet<FlashSaleItem> FlashSaleItems { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }


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

            entity.Property(e => e.Specifications).HasColumnName("specifications").HasColumnType("jsonb");
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

        // Payments
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.GatewayResponse).HasColumnType("jsonb");
            entity.HasOne(e => e.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Coupons
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("coupons");
            entity.HasKey(e => e.CouponId);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Order Status History
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(e => e.OrderId);
        });

        // Shipments
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("shipments");
            entity.HasKey(e => e.ShipmentId);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Shipments)
                .HasForeignKey(e => e.OrderId);
        });

        // Return Requests
        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.ToTable("return_requests");
            entity.HasKey(e => e.ReturnId);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.ReturnRequests)
                .HasForeignKey(e => e.OrderId);
        });

        // Return Request Items
        modelBuilder.Entity<ReturnRequestItem>(entity =>
        {
            entity.ToTable("return_request_items");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ReturnRequest)
                .WithMany(r => r.Items)
                .HasForeignKey(e => e.ReturnId);
        });

        // Return Request Images
        modelBuilder.Entity<ReturnRequestImage>(entity =>
        {
            entity.ToTable("return_request_images");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ReturnRequest)
                .WithMany(r => r.Images)
                .HasForeignKey(e => e.ReturnId);
        });

        // Wishlists
        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("wishlists");
            entity.HasKey(e => e.WishlistId);
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(e => e.ProductId);
        });

        // Coupon Usages
        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.ToTable("coupon_usages");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Coupon)
                .WithMany()
                .HasForeignKey(e => e.CouponId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId);
        });

        // Flash Sales
        modelBuilder.Entity<FlashSale>(entity =>
        {
            entity.ToTable("flash_sales");
            entity.HasKey(e => e.FlashSaleId);
        });

        // Flash Sale Items
        modelBuilder.Entity<FlashSaleItem>(entity =>
        {
            entity.ToTable("flash_sale_items");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FlashSaleId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.FlashSale)
                .WithMany(f => f.Items)
                .HasForeignKey(e => e.FlashSaleId);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.FlashSaleItems)
                .HasForeignKey(e => e.ProductId);
        });

        // Activity Logs
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");
            entity.HasKey(e => e.LogId);
        });
    }
}
