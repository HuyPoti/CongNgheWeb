using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetByProductId(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetByProductIdAsync(productId, page, pageSize, ct);
        return Ok(ApiResponse.Ok(reviews));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _reviewService.GetByIdAsync(id, ct);
        if (result == null) return NotFound(ApiResponse.Fail("Không tìm thấy đánh giá"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> Create([FromBody] CreateReviewDto dto, CancellationToken ct)
    {
        var result = await _reviewService.CreateAsync(dto, ct);
        if (result == null) return BadRequest(ApiResponse.Fail("Failed to create review"));
        return CreatedAtAction(nameof(GetById), new { id = result.ReviewId }, ApiResponse.Ok(result));
    }

    [HttpPatch("{id}/active")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateActive(Guid id, [FromBody] UpdateReviewActiveDto dto, CancellationToken ct)
    {
        var result = await _reviewService.UpdateActiveAsync(id, dto, ct);
        if (result == null) return NotFound(ApiResponse.Fail("Không tìm thấy đánh giá"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var success = await _reviewService.DeleteAsync(id, ct);
        if (!success) return NotFound(ApiResponse.Fail("Không tìm thấy đánh giá"));
        return Ok(ApiResponse.Ok(new { message = "Review deleted successfully" }));
    }

    [HttpPost("{reviewId}/replies")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ReviewReplyDto>>> CreateReply(Guid reviewId, [FromBody] CreateReviewReplyDto dto, CancellationToken ct)
    {
        var result = await _reviewService.CreateReplyAsync(reviewId, dto, ct);
        if (result == null) return NotFound(ApiResponse.Fail("Review không tồn tại"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPut("replies/{replyId}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<ReviewReplyDto>>> UpdateReply(Guid replyId, [FromBody] UpdateReviewReplyDto dto, CancellationToken ct)
    {
        var result = await _reviewService.UpdateReplyAsync(replyId, dto, ct);
        if (result == null) return NotFound(ApiResponse.Fail("Reply không tồn tại"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("replies/{replyId}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReply(Guid replyId, CancellationToken ct)
    {
        var success = await _reviewService.DeleteReplyAsync(replyId, ct);
        if (!success) return NotFound(ApiResponse.Fail("Reply not found"));
        return Ok(ApiResponse.Ok(new { message = "Reply deleted successfully" }));
    }

    [HttpPost("{reviewId}/images")]
    public async Task<ActionResult<ApiResponse<ReviewImageDto>>> AddImage(Guid reviewId, [FromBody] CreateReviewImageDto dto, CancellationToken ct)
    {
        var result = await _reviewService.AddImageAsync(reviewId, dto, ct);
        if (result == null) return NotFound(ApiResponse.Fail("Review không tồn tại"));
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("images/{imageId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage(Guid imageId, CancellationToken ct)
    {
        var success = await _reviewService.DeleteImageAsync(imageId, ct);
        if (!success) return NotFound(ApiResponse.Fail("Image not found"));
        return Ok(ApiResponse.Ok(new { message = "Image deleted successfully" }));
    }

    [HttpGet("{reviewId}/images")]
    public async Task<ActionResult<ApiResponse<List<ReviewImageDto>>>> GetImages(Guid reviewId, CancellationToken ct)
    {
        var result = await _reviewService.GetImagesByReviewIdAsync(reviewId, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("{reviewId}/votes/toggle")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleVote(Guid reviewId, [FromBody] ToggleVoteDto dto, CancellationToken ct)
    {
        var success = await _reviewService.ToggleVoteAsync(reviewId, dto, ct);
        if (!success) return NotFound(ApiResponse.Fail("Review không tồn tại"));

        var count = await _reviewService.GetVoteCountAsync(reviewId, ct);
        var hasVoted = await _reviewService.HasUserVotedAsync(reviewId, dto.UserId, ct);
        return Ok(ApiResponse.Ok(new { helpfulCount = count, hasVoted }));
    }

    [HttpGet("{reviewId}/votes/count")]
    public async Task<ActionResult<ApiResponse<object>>> GetVoteCount(Guid reviewId, CancellationToken ct)
    {
        var count = await _reviewService.GetVoteCountAsync(reviewId, ct);
        return Ok(ApiResponse.Ok(new { helpfulCount = count }));
    }

    [HttpGet("{reviewId}/votes/check/{userId}")]
    public async Task<ActionResult<ApiResponse<object>>> CheckUserVoted(Guid reviewId, Guid userId, CancellationToken ct)
    {
        var hasVoted = await _reviewService.HasUserVotedAsync(reviewId, userId, ct);
        return Ok(ApiResponse.Ok(new { hasVoted }));
    }
}