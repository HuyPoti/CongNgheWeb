using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs;

public class CreateBannerDto
{
    [Required(ErrorMessage = "Tiêu đề banner là bắt buộc")]
    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    [Required(ErrorMessage = "Ảnh banner là bắt buộc")]
    public string ImageUrl { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    [Required]
    public BannerPosition Position { get; set; } = BannerPosition.homepage_slider;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}