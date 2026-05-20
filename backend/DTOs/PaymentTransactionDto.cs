namespace backend.DTOs;

public class PaymentTransactionDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public int Status { get; set; } // 1=pending, 2=success, 3=failed, 4=refunded
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
