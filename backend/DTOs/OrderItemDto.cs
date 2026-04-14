namespace backend.DTOs;

public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid ProductId { get; set; }

    // From Product.Name
    public string ProductName { get; set; } = string.Empty;
    // From Product.ImageUrl
    public string? ProductImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
