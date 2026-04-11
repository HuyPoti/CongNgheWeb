using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }

    // Them dbset products, product_images, product_spec
    public DbSet<Product> Products { get; set; }

    public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<ProductSpec> Specs { get; set; }

    public DbSet<Banner> Banners { get; set; }

    public DbSet<Brand> Brands { get; set; }

    public DbSet<News> News { get; set; }

    public DbSet<NewsCategory> NewsCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //--------------------------------------------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(e => e.Email).IsUnique();

            // Ánh xạ cột role tới INT trong database (1: admin, 2: staff, 3: customer)
            // EF Core mặc định mapping enum C# sang int (integer)
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .IsRequired();
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.ToTable("banners");
            entity.Property(e => e.ImageUrl).HasColumnType("text");
            entity.Property(e => e.LinkUrl).HasColumnType("text");
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
        }); // Đóng đúng ngoặc cho Banner
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.BrandId);
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            
            // Cấu hình tường minh cho cột jsonb
            entity.Property(e => e.Specifications)
                .HasColumnName("specifications")
                .HasColumnType("jsonb");

            entity.HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);
        });
        // PRODUCT IMAGES
        modelBuilder.Entity<ProductImage>()
        .HasOne(pi => pi.Product)
        .WithMany(p => p.Images)
        .HasForeignKey(pi => pi.ProductId);
        // Relation SPEC
        modelBuilder.Entity<ProductSpec>()
        .HasOne(ps => ps.Product)
        .WithMany(p => p.Specs)
        .HasForeignKey(ps => ps.ProductId);


    }

}