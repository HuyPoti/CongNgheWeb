using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ShipmentDto
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string? TrackingCode { get; set; }
    public decimal ShippingFee { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Status { get; set; } = "pending"; // pending, packing, qc_passed, shipping, delivered
    public bool QcPassed { get; set; }
    public string? QcNotes { get; set; }
    public Guid? PackedBy { get; set; }
    public string? PackedByName { get; set; }
    public DateTime? PackedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateShipmentDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Carrier { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TrackingCode { get; set; }

    public decimal ShippingFee { get; set; } = 0;

    public DateTime? EstimatedDelivery { get; set; }
}

public class UpdateShipmentDto
{
    [MaxLength(50)]
    public string? Carrier { get; set; }

    [MaxLength(100)]
    public string? TrackingCode { get; set; }

    public decimal? ShippingFee { get; set; }

    public DateTime? EstimatedDelivery { get; set; }

    public DateTime? ActualDelivery { get; set; }
}

public class MarkQcDto
{
    [Required]
    public bool QcPassed { get; set; }

    public string? QcNotes { get; set; }
}



public class CancelOrderDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
