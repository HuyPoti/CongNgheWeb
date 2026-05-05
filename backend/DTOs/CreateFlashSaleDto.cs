namespace backend.DTOs;

public class CreateFlashSaleDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedBy { get; set; }
}

public class CreateFlashSaleItemDto
{
    public Guid ProductId { get; set; }
    public decimal FlashPrice { get; set; }
    public int StockLimit { get; set; }
}
