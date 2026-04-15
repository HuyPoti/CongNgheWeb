namespace backend.DTOs;

public class UpdateNewsDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Excerpt { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPublished { get; set; }
}