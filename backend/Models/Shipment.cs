using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("shipments")]
public class Shipment
{
    [Key]
    [Column("shipment_id")]
    public Guid ShipmentId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("carrier")]
    public string Carrier { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("tracking_code")]
    public string? TrackingCode { get; set; }

    [Column("shipping_fee")]
    public decimal ShippingFee { get; set; } = 0;

    [Column("estimated_delivery")]
    public DateTime? EstimatedDelivery { get; set; }

    [Column("actual_delivery")]
    public DateTime? ActualDelivery { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("packed_by")]
    public Guid? PackedBy { get; set; }

    [Column("packed_at")]
    public DateTime? PackedAt { get; set; }

    [Column("qc_passed")]
    public bool QcPassed { get; set; } = false;

    [Column("qc_notes")]
    public string? QcNotes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [ForeignKey("PackedBy")]
    public User? PackedByUser { get; set; }
}
