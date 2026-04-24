

using Microsoft.AspNetCore.SignalR;

namespace backend.DTOs;

public class ProductFullDto
{
    public ProductDto Product { get; set; } = default!;
    public List<ProductImageDto> Images { get; set; } = new();
}