namespace backend.DTOs;

public class ReviewHelpfulVoteDto
{
    public Guid VoteId {get; set; }
    public Guid UserId {get; set; }
    public string UserName {get; set; } = null!;
    public DateTime CreatedAt {get; set; }
}