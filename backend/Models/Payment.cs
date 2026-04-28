using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("payments")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    public Guid PaymentId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("transaction_id")]
    public string? TransactionId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("gateway_response", TypeName = "jsonb")]
    public string? GatewayResponse { get; set; }

    [MaxLength(500)]
    [Column("return_url")]
    public string? ReturnUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
}
