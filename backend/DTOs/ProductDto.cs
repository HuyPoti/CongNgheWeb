



using Microsoft.AspNetCore.SignalR;

namespace backend.DTOs;



public class ProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? Sku { get; set; } = string.Empty;


    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public int StockQuantity { get; set; }

    public int WarrantyMonths { get; set; }

    // Note
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Specifications { get; set; }

    public int Status { get; set; } = 1;

    public Guid BrandId { get; set; }

    public DateTime CreatedAt { get; set; }
}