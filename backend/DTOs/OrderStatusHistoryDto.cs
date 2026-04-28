namespace backend.DTOs;

public class OrderStatusHistoryDto
{
    public Guid Id { get; set; }
    public int OldStatus { get; set; }
    public int NewStatus { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
