using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("review_helpful_votes")]
public class ReviewHelpfulVote
{
    [Key]
    [Column("vote_id")]
    public Guid VoteId { get; set; }

    [Required]
    [Column("review_id")]
    public Guid ReviewId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt {get; set; }

    [ForeignKey("ReviewId")]
    public Review Review {get; set; } = null!;

    [ForeignKey("UserId")]
    public User User {get; set; } = null!;
}