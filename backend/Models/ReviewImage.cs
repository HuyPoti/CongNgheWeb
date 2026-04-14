using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("review_images")]
public class ReviewImage
{
    [Key]
    [Column("image_id")]
    public Guid ImageId { get; set; }

    [Required]
    [Column("review_id")]
    public Guid ReviewId { get; set; }

    [Required]
    [Column("image_url")]
    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ReviewId")]
    public Review Review { get; set; } = null!;
}
