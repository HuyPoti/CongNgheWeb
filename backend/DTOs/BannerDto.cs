using backend.Models;

namespace backend.DTOs;

public class BannerDto
{
    public Guid BannerId { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public BannerPosition Position { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}