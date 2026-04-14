namespace backend.DTOs;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;

    // 1: pending - 2: confirmed - 3: processing
    // 4: shipping - 5: delivered - 6: cancelled
    public string Status { get; set; } = "pending";

    public string PaymentMethod { get; set; } = string.Empty;

    // 1: unpaid - 2: paid - 3: refunded
    public string PaymentStatus { get; set; } = "unpaid";

    public decimal TotalAmount { get; set; }

    // From Order.User.FullName
    public string CustomerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
