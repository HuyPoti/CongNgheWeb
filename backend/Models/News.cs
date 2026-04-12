using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("news")]
public class News
{
    [Key]
    [Column("news_id")]
    public Guid NewsId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Required] [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required] [MaxLength(255)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("excerpt")]
    public string? Excerpt { get; set; }

    [Required]
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [MaxLength(500)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_published")]
    public bool IsPublished { get; set; } = false;

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("views")]
    public int Views { get; set; } = 0;

    [MaxLength(255)]
    [Column("meta_title")]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    [Column("meta_description")]
    public string? MetaDescription { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CategoryId")]
    public NewsCategory? Category { get; set; }

    [ForeignKey("AuthorId")]
    public User? Author { get; set; }
}