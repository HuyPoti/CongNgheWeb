namespace backend.DTOs;

public class ReviewReplyDto
{
    public Guid ReplyId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
