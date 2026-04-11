using System.ComponentModel.DataAnnotations;
namespace backend.DTOs;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug là bắt buộc")]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Guid? ParentId { get; set; }  
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;  
}