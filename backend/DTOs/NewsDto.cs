namespace backend.DTOs;

public class NewsDto
{
    public Guid NewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public bool IsPublished { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
}