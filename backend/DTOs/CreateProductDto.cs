namespace backend.DTOs;

public class CreateProductDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public int StockQuantity { get; set; }
    public int WarrantyMonths { get; set; }

    public string? Description { get; set; }

    public string? Specifications { get; set; }

    public int? Status { get; set; }
    public Guid BrandId { get; set; }
}