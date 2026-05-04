using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateReturnRequestDto
{
    [Required]
    [RegularExpression("approved|rejected|completed")]
    public string Status { get; set; } = string.Empty;

    public decimal? RefundAmount { get; set; }

    public string? AdminNote { get; set; }
}
