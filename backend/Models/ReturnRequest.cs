using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("return_requests")]
public class ReturnRequest
{
    [Key]
    [Column("return_id")]
    public Guid ReturnId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("refund_amount")]
    public decimal? RefundAmount { get; set; }

    [Column("processed_by")]
    public Guid? ProcessedBy { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Column("admin_note")]
    public string? AdminNote { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("ProcessedBy")]
    public User? ProcessedByUser { get; set; }

    public ICollection<ReturnRequestItem> Items { get; set; } = new List<ReturnRequestItem>();
    public ICollection<ReturnRequestImage> Images { get; set; } = new List<ReturnRequestImage>();
}
