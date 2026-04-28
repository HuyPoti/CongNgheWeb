namespace backend.DTOs;

public class UpdateOrderDto
{
    // Status: pending, confirmed, processing, shipping, delivered, cancelled
    public string? Status { get; set; }

    // PaymentStatus: unpaid, paid, refunded
    public string? PaymentStatus { get; set; }

    public string? CancelledReason { get; set; }
}
