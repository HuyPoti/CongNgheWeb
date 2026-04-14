using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("review_replies")]
public class ReviewReply
{
    [Key]
    [Column("reply_id")]
    public Guid ReplyId { get; set; }

    [Required]
    [Column("review_id")]
    public Guid ReviewId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; } // The admin/staff who replied

    [Required]
    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("is_active")]
    public int IsActive { get; set; } = 1;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ReviewId")]
    public Review Review { get; set; } = null!;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
