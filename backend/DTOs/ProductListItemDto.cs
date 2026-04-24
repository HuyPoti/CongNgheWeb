namespace backend.DTOs;

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategorySlug { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string BrandName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }

    public int StockQuantity { get; set; }
    public int WarrantyMonths { get; set; }
}

