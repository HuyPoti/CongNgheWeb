namespace backend.DTOs;

public class UpdateBrandDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
