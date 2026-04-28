namespace backend.DTOs;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    public Guid? UserId { get; set; }
    public Guid? ShippingAddressId { get; set; }
    public CreateOrderShippingAddressDto? ShippingAddress { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public decimal ShippingFee { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderShippingAddressDto
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
}
