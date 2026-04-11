using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("news_categories")]
public class NewsCategory
{
    [Key]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Required] [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required] [MaxLength(100)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("parent_id")]
    public Guid? ParentId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ParentId")]
    public NewsCategory? Parent { get; set; }

    [InverseProperty("Parent")]
    public List<NewsCategory> Children { get; set; } = new();

    public List<News> News { get; set; } = new();
}