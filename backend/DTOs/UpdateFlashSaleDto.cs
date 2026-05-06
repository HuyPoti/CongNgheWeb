namespace backend.DTOs;

public class UpdateFlashSaleDto
{
    public string? Title { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsActive { get; set; }
}
