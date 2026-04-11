


namespace backend.DTOs;

public class ProductSpecDto
{
    public Guid SpecId { get; set; }
    public string SpecKey { get; set; } = string.Empty;
    public string SpecValue { get; set; } = string.Empty;
}