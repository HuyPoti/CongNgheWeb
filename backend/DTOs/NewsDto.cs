namespace backend.DTOs;

public class NewsDto
{
    public Guid NewsId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Views { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime CreatedAt { get; set; }
}