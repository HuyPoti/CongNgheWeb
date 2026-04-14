namespace backend.DTOs;

public class ReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int IsActive { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ReviewImageDto> Images { get; set; } = new();
    public List<ReviewReplyDto> Replies { get; set; } = new();
    public List<ReviewHelpfulVoteDto> HelpfulVotes { get; set; } = new();
    public int HelpfulVoteCount { get; set; }
}

public class UpdateReviewActiveDto
{
    public int IsActive { get; set; }
}
