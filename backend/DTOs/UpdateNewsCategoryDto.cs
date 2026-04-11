namespace backend.DTOs;

public class UpdateNewsCategoryDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsActive { get; set; }
}
