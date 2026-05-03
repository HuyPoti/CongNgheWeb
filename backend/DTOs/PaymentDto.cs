namespace backend.DTOs;

public class PaymentDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;  // cod, vnpay, bank_transfer
    public string? TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;  // pending, success, failed, refunded
    public string? GatewayResponse { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
