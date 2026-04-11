using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;


[Table("products")]
public class Product
{


    // Tam thoi chua them brand_id
    [Key]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("sku")]
    public string? Sku { get; set; }

    [Required]
    [Column("regular_price")]
    public decimal RegularPrice { get; set; }

    [Column("sale_price")]
    public decimal? SalePrice { get; set; }


    [Column("stock_quantity")]
    public int StockQuantity { get; set; } = 0;

    [Column("warranty_months")]
    public int WarrantyMonths { get; set; } = 0;

    [Column("description")]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Specifications { get; set; }


    [Column("status")]
    public int Status { get; set; } = 1; // 1: draft, 2: published, 3: deleted


    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("brand_id")]
    public Guid BrandId { get; set; }

    [ForeignKey("BrandId")]
    public Brand Brand { get; set; } = null!;

    // Navigation
    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductSpec> Specs { get; set; } = new List<ProductSpec>();
}