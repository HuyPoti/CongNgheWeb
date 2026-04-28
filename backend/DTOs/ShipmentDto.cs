namespace backend.DTOs;

public class ShipmentDto
{
    public Guid ShipmentId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string? TrackingCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
}
