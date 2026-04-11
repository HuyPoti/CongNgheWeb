namespace backend.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Sku { get; set; }

    public Guid? CategoryId { get; set; }

    public decimal? RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public int? StockQuantity { get; set; }
    public int? WarrantyMonths { get; set; }

    public string? Description { get; set; }

    public string? Specifications { get; set; }

    public int? Status { get; set; }
    public Guid? BrandId { get; set; }
}