namespace backend.DTOs;

public class CreateReviewReplyDto
{
    public Guid UserId {get; set; }
    public string Content {get; set; } = null!;
}