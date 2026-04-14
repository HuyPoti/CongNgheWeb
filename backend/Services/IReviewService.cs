using backend.DTOs;

namespace backend.Services;

public interface IReviewService
{
    Task<List<ReviewDto>> GetAllAsync(CancellationToken ct);
    Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ReviewDto?> UpdateActiveAsync(Guid id, UpdateReviewActiveDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    // === Review Replies ===
    Task<ReviewReplyDto?> CreateReplyAsync(Guid reviewId, CreateReviewReplyDto dto, CancellationToken ct);
    Task<ReviewReplyDto?> UpdateReplyAsync(Guid replyId, UpdateReviewReplyDto dto, CancellationToken ct);
    Task<bool> DeleteReplyAsync(Guid replyId, CancellationToken ct);

    // === Review Images ===
    Task<ReviewImageDto?> AddImageAsync(Guid reviewId, CreateReviewImageDto dto, CancellationToken ct);
    Task<bool> DeleteImageAsync(Guid imageId, CancellationToken ct);
    Task<List<ReviewImageDto>> GetImagesByReviewIdAsync(Guid reviewId, CancellationToken ct);

    // === Review Helpful Votes ===
    Task<bool> ToggleVoteAsync(Guid reviewId, ToggleVoteDto dto, CancellationToken ct);
    Task<int> GetVoteCountAsync(Guid reviewId, CancellationToken ct);
    Task<bool> HasUserVotedAsync(Guid reviewId, Guid userId, CancellationToken ct);
}
