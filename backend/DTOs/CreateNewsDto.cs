namespace backend.DTOs;

public class CreateNewsDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid AuthorId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Excerpt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; }
}