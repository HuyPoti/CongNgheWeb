using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateBrandDto
{
    [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug là bắt buộc")]
    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}
