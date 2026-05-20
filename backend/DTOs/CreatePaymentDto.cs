namespace backend.DTOs;

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }  // Required - ID đơn hàng
    public string PaymentMethod { get; set; } = string.Empty;  // cod, vnpay, bank_transfer
    public string? ReturnUrl { get; set; }  // URL redirect sau khi thanh toán
}

public class CreatePaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }  // URL redirect cho VNPay
    public string? BankInfo { get; set; }  // Thông tin chuyển khoản cho bank_transfer
    public string? QrUrl { get; set; }  // Link mã QR động VietQR
}
