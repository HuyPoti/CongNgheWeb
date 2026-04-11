using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("categories")]
public class Category
{
    [Key]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("parent_id")]
    public Guid? ParentId { get; set; }

    [MaxLength(255)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ParentId")]
    public Category? Parent { get; set; }

    [InverseProperty("Parent")]
    public List<Category> Children { get; set; } = new();

    // Update quan he 1-n voi products
    public List<Product> Products { get; set; } = new();
}