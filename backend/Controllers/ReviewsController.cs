using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _reviewService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _reviewService.GetByIdAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> UpdateActive(Guid id, [FromBody] UpdateReviewActiveDto dto, CancellationToken ct)
    {
        var result = await _reviewService.UpdateActiveAsync(id, dto, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var success = await _reviewService.DeleteAsync(id, ct);
        if (!success) return NotFound();
        return NoContent();
    }

    // ============================
    // REVIEW REPLIES
    // ============================

    // POST api/reviews/{reviewId}/replies
    [HttpPost("{reviewId}/replies")]
    public async Task<IActionResult> CreateReply(Guid reviewId, [FromBody] CreateReviewReplyDto dto, CancellationToken ct)
    {
        var result = await _reviewService.CreateReplyAsync(reviewId, dto, ct);
        if (result == null) return NotFound("Review không tồn tại");
        return Ok(result);
    }

    // PUT api/reviews/replies/{replyId}
    [HttpPut("replies/{replyId}")]
    public async Task<IActionResult> UpdateReply(Guid replyId, [FromBody] UpdateReviewReplyDto dto, CancellationToken ct)
    {
        var result = await _reviewService.UpdateReplyAsync(replyId, dto, ct);
        if (result == null) return NotFound("Reply không tồn tại");
        return Ok(result);
    }

    // DELETE api/reviews/replies/{replyId}
    [HttpDelete("replies/{replyId}")]
    public async Task<IActionResult> DeleteReply(Guid replyId, CancellationToken ct)
    {
        var success = await _reviewService.DeleteReplyAsync(replyId, ct);
        if (!success) return NotFound();
        return NoContent();
    }

    // ============================
    // REVIEW IMAGES
    // ============================

    // POST api/reviews/{reviewId}/images
    [HttpPost("{reviewId}/images")]
    public async Task<IActionResult> AddImage(Guid reviewId, [FromBody] CreateReviewImageDto dto, CancellationToken ct)
    {
        var result = await _reviewService.AddImageAsync(reviewId, dto, ct);
        if (result == null) return NotFound("Review không tồn tại");
        return Ok(result);
    }

    // DELETE api/reviews/images/{imageId}
    [HttpDelete("images/{imageId}")]
    public async Task<IActionResult> DeleteImage(Guid imageId, CancellationToken ct)
    {
        var success = await _reviewService.DeleteImageAsync(imageId, ct);
        if (!success) return NotFound();
        return NoContent();
    }

    // GET api/reviews/{reviewId}/images
    [HttpGet("{reviewId}/images")]
    public async Task<IActionResult> GetImages(Guid reviewId, CancellationToken ct)
    {
        var result = await _reviewService.GetImagesByReviewIdAsync(reviewId, ct);
        return Ok(result);
    }

    // ============================
    // REVIEW HELPFUL VOTES
    // ============================

    // POST api/reviews/{reviewId}/votes/toggle
    [HttpPost("{reviewId}/votes/toggle")]
    public async Task<IActionResult> ToggleVote(Guid reviewId, [FromBody] ToggleVoteDto dto, CancellationToken ct)
    {
        var success = await _reviewService.ToggleVoteAsync(reviewId, dto, ct);
        if (!success) return NotFound("Review không tồn tại");

        var count = await _reviewService.GetVoteCountAsync(reviewId, ct);
        var hasVoted = await _reviewService.HasUserVotedAsync(reviewId, dto.UserId, ct);
        return Ok(new { helpfulCount = count, hasVoted });
    }

    // GET api/reviews/{reviewId}/votes/count
    [HttpGet("{reviewId}/votes/count")]
    public async Task<IActionResult> GetVoteCount(Guid reviewId, CancellationToken ct)
    {
        var count = await _reviewService.GetVoteCountAsync(reviewId, ct);
        return Ok(new { helpfulCount = count });
    }

    // GET api/reviews/{reviewId}/votes/check/{userId}
    [HttpGet("{reviewId}/votes/check/{userId}")]
    public async Task<IActionResult> CheckUserVoted(Guid reviewId, Guid userId, CancellationToken ct)
    {
        var hasVoted = await _reviewService.HasUserVotedAsync(reviewId, userId, ct);
        return Ok(new { hasVoted });
    }
}