namespace backend.DTOs;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    public Guid UserId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
