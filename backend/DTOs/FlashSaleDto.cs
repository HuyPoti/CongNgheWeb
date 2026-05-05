namespace backend.DTOs;

public class FlashSaleDto
{
    public Guid FlashSaleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FlashSaleItemDto> Items { get; set; } = new();
}

public class FlashSaleItemDto
{
    public Guid Id { get; set; }
    public Guid FlashSaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal FlashPrice { get; set; }
    public int StockLimit { get; set; }
    public int SoldCount { get; set; }
    public bool IsSoldOut { get; set; }
}
