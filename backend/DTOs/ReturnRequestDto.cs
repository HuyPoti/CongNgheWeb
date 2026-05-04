namespace backend.DTOs;

public class ReturnRequestDto
{
    public Guid ReturnId { get; set; }
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending"; // pending, approved, rejected, completed
    public decimal? RefundAmount { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ReturnRequestItemDto> Items { get; set; } = new();
    public List<ReturnRequestImageDto> Images { get; set; } = new();
}

public class ReturnRequestItemDto
{
    public Guid Id { get; set; }
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ReasonDetail { get; set; }
}

public class ReturnRequestImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
