using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public enum BannerPosition
{
    homepage_slider,
    homepage_mid,
    category_top,
    news_top
}

[Table("banners")]
public class Banner
{
    [Key]
    [Column("banner_id")]
    public Guid BannerId { get; set; }

    [MaxLength(255)]
    [Column("title")]
    public string? Title { get; set; }

    [MaxLength(255)]
    [Column("subtitle")]
    public string? Subtitle { get; set; }

    [Required]
    [Column("image_url", TypeName = "text")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("link_url", TypeName = "text")]
    public string? LinkUrl { get; set; }

    [Column("position")]
    public BannerPosition Position { get; set; } = BannerPosition.homepage_slider;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}