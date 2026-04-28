using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("order_status_history")]
public class OrderStatusHistory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("old_status")]
    public int? OldStatus { get; set; }

    [Required]
    [Column("new_status")]
    public int NewStatus { get; set; }

    [Required]
    [Column("changed_by")]
    public Guid ChangedBy { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [ForeignKey("ChangedBy")]
    public User? ChangedByUser { get; set; }
}
