namespace backend.DTOs;

public class VnPayRequestDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class VnPayResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public string? TransactionNo { get; set; }
}
