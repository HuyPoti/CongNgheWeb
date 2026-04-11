using backend.Models;

namespace backend.DTOs;

public class UpdateBannerDto
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public BannerPosition? Position { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}