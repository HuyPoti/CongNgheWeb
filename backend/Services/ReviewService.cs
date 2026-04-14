using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ReviewService(IUnitOfWork uow, IMapper mapper) : IReviewService
{
    public async Task<List<ReviewDto>> GetAllAsync(CancellationToken ct)
    {
        var reviews = await uow.Reviews.Query()
            .Include(r => r.Product)
            .Include(r => r.User)
            .Include(r => r.Images)
            .Include(r => r.Replies).ThenInclude(rp => rp.User)
            .Include(r => r.HelpfulVotes).ThenInclude(v => v.User)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectTo<ReviewDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return reviews;
    }

    public async Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var review = await uow.Reviews.Query()
            .Include(r => r.Product)
            .Include(r => r.User)
            .Include(r => r.Images)
            .Include(r => r.Replies).ThenInclude(rp => rp.User)
            .Include(r => r.HelpfulVotes).ThenInclude(v => v.User)
            .ProjectTo<ReviewDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(r => r.ReviewId == id, ct);
        return review;
    }

    public async Task<ReviewDto?> UpdateActiveAsync(Guid id, UpdateReviewActiveDto dto, CancellationToken ct)
    {
        var entity = await uow.Reviews.Query().FirstOrDefaultAsync(r => r.ReviewId == id, ct);
        if (entity == null) return null;

        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.Reviews.Update(entity);
        await uow.SaveAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.Reviews.Query().FirstOrDefaultAsync(r => r.ReviewId == id, ct);
        if (entity == null) return false;

        uow.Reviews.Delete(entity);
        await uow.SaveAsync(ct);
        return true;
    }

    public async Task<ReviewReplyDto?> CreateReplyAsync(Guid reviewId, CreateReviewReplyDto dto, CancellationToken ct)
    {
        // Kiểm tra review tồn tại
        var review = await uow.Reviews.Query().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        if (review == null) return null;

        var entity = new ReviewReply
        {
            ReplyId = Guid.NewGuid(),
            ReviewId = reviewId,
            UserId = dto.UserId,
            Content = dto.Content,
            IsActive = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        uow.ReviewReplies.Insert(entity);
        await uow.SaveAsync(ct);

        // Load lại với User để lấy FullName
        var result = await uow.ReviewReplies.Query()
            .Include(rp => rp.User)
            .ProjectTo<ReviewReplyDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(rp => rp.ReplyId == entity.ReplyId, ct);

        return result;
    }

    public async Task<ReviewReplyDto?> UpdateReplyAsync(Guid replyId, UpdateReviewReplyDto dto, CancellationToken ct)
    {
        var entity = await uow.ReviewReplies.Query().FirstOrDefaultAsync(rp => rp.ReplyId == replyId, ct);
        if (entity == null) return null;

        entity.Content = dto.Content;
        entity.UpdatedAt = DateTime.UtcNow;
        uow.ReviewReplies.Update(entity);
        await uow.SaveAsync(ct);

        var result = await uow.ReviewReplies.Query()
            .Include(rp => rp.User)
            .ProjectTo<ReviewReplyDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(rp => rp.ReplyId == replyId, ct);

        return result;
    }

    public async Task<bool> DeleteReplyAsync(Guid replyId, CancellationToken ct)
    {
        var entity = await uow.ReviewReplies.Query().FirstOrDefaultAsync(rp => rp.ReplyId == replyId, ct);
        if (entity == null) return false;

        uow.ReviewReplies.Delete(entity);
        await uow.SaveAsync(ct);
        return true;
    }

    public async Task<ReviewImageDto?> AddImageAsync(Guid reviewId, CreateReviewImageDto dto, CancellationToken ct)
    {
        var review = await uow.Reviews.Query().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        if (review == null) return null;

        var entity = new ReviewImage
        {
            ImageId = Guid.NewGuid(),
            ReviewId = reviewId,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        uow.ReviewImages.Insert(entity);
        await uow.SaveAsync(ct);

        return mapper.Map<ReviewImageDto>(entity);
    }

    public async Task<bool> DeleteImageAsync(Guid imageId, CancellationToken ct)
    {
        var entity = await uow.ReviewImages.Query().FirstOrDefaultAsync(i => i.ImageId == imageId, ct);
        if (entity == null) return false;

        uow.ReviewImages.Delete(entity);
        await uow.SaveAsync(ct);
        return true;
    }

    public async Task<List<ReviewImageDto>> GetImagesByReviewIdAsync(Guid reviewId, CancellationToken ct)
    {
        return await uow.ReviewImages.Query()
            .Where(i => i.ReviewId == reviewId)
            .OrderBy(i => i.CreatedAt)
            .ProjectTo<ReviewImageDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<bool> ToggleVoteAsync(Guid reviewId, ToggleVoteDto dto, CancellationToken ct)
    {
        var review = await uow.Reviews.Query().FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);
        if (review == null) return false;

        var existingVote = await uow.ReviewHelpfulVotes.Query()
            .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == dto.UserId, ct);

        if (existingVote != null)
        {
            // User đã vote → Huỷ vote
            uow.ReviewHelpfulVotes.Delete(existingVote);
        }
        else
        {
            // Chưa vote → Thêm vote
            var vote = new ReviewHelpfulVote
            {
                VoteId = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };
            uow.ReviewHelpfulVotes.Insert(vote);
        }

        await uow.SaveAsync(ct);
        return true;
    }

    public async Task<int> GetVoteCountAsync(Guid reviewId, CancellationToken ct)
    {
        return await uow.ReviewHelpfulVotes.Query()
            .CountAsync(v => v.ReviewId == reviewId, ct);
    }

    public async Task<bool> HasUserVotedAsync(Guid reviewId, Guid userId, CancellationToken ct)
    {
        return await uow.ReviewHelpfulVotes.Query()
            .AnyAsync(v => v.ReviewId == reviewId && v.UserId == userId, ct);
    }
}
