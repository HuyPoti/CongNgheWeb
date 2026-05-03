namespace backend.DTOs;

public class PaymentDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;  // cod, vnpay, bank_transfer
    public string? TransactionId { get; set; }
    public int Status { get; set; }  // 1=pending, 2=success, 3=failed, 4=refunded
    public string? GatewayResponse { get; set; }
    public string? ReturnUrl { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
