namespace backend.DTOs;

public class ProductQueryDto : BaseQueryDto
{
    public string? Keyword { get; set; }
    public string? CategorySlug { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // price_asc, price_desc, name_asc, latest
}
