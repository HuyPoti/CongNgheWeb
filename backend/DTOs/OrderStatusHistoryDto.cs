namespace backend.DTOs;

public class OrderStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int? OldStatus { get; set; }
    public string? OldStatusLabel { get; set; }
    public int NewStatus { get; set; }
    public string NewStatusLabel { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
