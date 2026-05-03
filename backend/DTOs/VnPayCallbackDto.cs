namespace backend.DTOs;

// DTO nhận callback từ VNPay sau khi khách thanh toán
public class VnPayCallbackDto
{
    // Mã tham chiếu giao dịch (OrderId của hệ thống)
    public string? vnp_TxnRef { get; set; }

    // Số tiền thanh toán (VNPay trả về theo đơn vị VND x 100)
    public string? vnp_Amount { get; set; }

    // Mã phản hồi từ VNPay (00 = thành công)
    public string? vnp_ResponseCode { get; set; }

    // Mã giao dịch của VNPay
    public string? vnp_TransactionNo { get; set; }

    // Mã ngân hàng thanh toán
    public string? vnp_BankCode { get; set; }

    // Thời gian thanh toán (yyyyMMddHHmmss)
    public string? vnp_PayDate { get; set; }

    // Chữ ký bảo mật để kiểm tra tính toàn vẹn
    public string? vnp_SecureHash { get; set; }
}

// Response trả về sau khi xử lý callback
public class VnPayResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? TransactionId { get; set; }
    public decimal? Amount { get; set; }
}
