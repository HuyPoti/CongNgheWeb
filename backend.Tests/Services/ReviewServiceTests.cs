using AutoMapper;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using backend.MapperProfiles;

namespace backend.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly IMapper _mapper;
    private readonly ReviewService _service;
    private readonly Mock<IRepository<Review>> _mockReviewRepo;
    private readonly Mock<IRepository<ReviewImage>> _mockImageRepo;
    private readonly Mock<IRepository<ReviewReply>> _mockReplyRepo;
    private readonly Mock<IRepository<ReviewHelpfulVote>> _mockVoteRepo;
    private readonly Mock<IRepository<Product>> _mockProductRepo;
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<Order>> _mockOrderRepo;

    public ReviewServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(ReviewProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        _mockReviewRepo = new Mock<IRepository<Review>>();
        _mockImageRepo = new Mock<IRepository<ReviewImage>>();
        _mockReplyRepo = new Mock<IRepository<ReviewReply>>();
        _mockVoteRepo = new Mock<IRepository<ReviewHelpfulVote>>();
        _mockProductRepo = new Mock<IRepository<Product>>();
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockOrderRepo = new Mock<IRepository<Order>>();

        _mockUow.Setup(u => u.Reviews).Returns(_mockReviewRepo.Object);
        _mockUow.Setup(u => u.ReviewImages).Returns(_mockImageRepo.Object);
        _mockUow.Setup(u => u.ReviewReplies).Returns(_mockReplyRepo.Object);
        _mockUow.Setup(u => u.ReviewHelpfulVotes).Returns(_mockVoteRepo.Object);
        _mockUow.Setup(u => u.Products).Returns(_mockProductRepo.Object);
        _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);
        _mockUow.Setup(u => u.Orders).Returns(_mockOrderRepo.Object);

        _service = new ReviewService(_mockUow.Object, _mapper);
    }

    // ============================================================
    // ToggleVoteAsync
    // ============================================================

    [Fact]
    public async Task ToggleVoteAsync_ReviewNotFound_ReturnsFalse()
    {
        var reviews = new List<Review>().AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.ToggleVoteAsync(Guid.NewGuid(), new ToggleVoteDto { UserId = Guid.NewGuid() }, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleVoteAsync_NoExistingVote_AddsVote()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reviews = new List<Review> { new() { ReviewId = reviewId } }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var votes = new List<ReviewHelpfulVote>().AsQueryable().BuildMock();
        _mockVoteRepo.Setup(r => r.Query()).Returns(votes);
        _mockVoteRepo.Setup(r => r.Insert(It.IsAny<ReviewHelpfulVote>())).Returns(new ReviewHelpfulVote());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.ToggleVoteAsync(reviewId, new ToggleVoteDto { UserId = userId }, CancellationToken.None);

        result.Should().BeTrue();
        _mockVoteRepo.Verify(r => r.Insert(It.IsAny<ReviewHelpfulVote>()), Times.Once);
    }

    [Fact]
    public async Task ToggleVoteAsync_ExistingVote_RemovesVote()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingVote = new ReviewHelpfulVote { VoteId = Guid.NewGuid(), ReviewId = reviewId, UserId = userId };

        var reviews = new List<Review> { new() { ReviewId = reviewId } }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var votes = new List<ReviewHelpfulVote> { existingVote }.AsQueryable().BuildMock();
        _mockVoteRepo.Setup(r => r.Query()).Returns(votes);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.ToggleVoteAsync(reviewId, new ToggleVoteDto { UserId = userId }, CancellationToken.None);

        result.Should().BeTrue();
        _mockVoteRepo.Verify(r => r.Delete(existingVote), Times.Once);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_InvalidProductId_ThrowsBadRequest()
    {
        var dto = new CreateReviewDto { ProductId = "not-a-guid", UserId = Guid.NewGuid().ToString(), Rating = 5 };
        var act = () => _service.CreateAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*ProductId*");
    }

    [Fact]
    public async Task CreateAsync_ProductNotFound_ThrowsNotFound()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByIdAsync<Product>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var dto = new CreateReviewDto { ProductId = productId.ToString(), UserId = userId.ToString(), Rating = 5 };
        var act = () => _service.CreateAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Product*");
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ThrowsNotFound()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByIdAsync<Product>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = productId, Name = "P", Slug = "p", Sku = "S" });
        _mockUserRepo.Setup(r => r.GetByIdAsync<User>(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var dto = new CreateReviewDto { ProductId = productId.ToString(), UserId = userId.ToString(), Rating = 5 };
        var act = () => _service.CreateAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*User*");
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesReview()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByIdAsync<Product>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = productId, Name = "Prod", Slug = "p", Sku = "S" });
        _mockUserRepo.Setup(r => r.GetByIdAsync<User>(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = userId, FullName = "User", Email = "u@u.com" });

        var orders = new List<Order>().AsQueryable().BuildMock();
        _mockOrderRepo.Setup(r => r.Query()).Returns(orders);

        _mockReviewRepo.Setup(r => r.Insert(It.IsAny<Review>())).Returns(new Review());
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateReviewDto
        {
            ProductId = productId.ToString(),
            UserId = userId.ToString(),
            Rating = 4,
            Comment = "Great product"
        };

        var result = await _service.CreateAsync(dto, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Rating.Should().Be(4);
        result.IsVerifiedPurchase.Should().BeFalse(); // No delivered order
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        var reviews = new List<Review>().AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Found_DeletesAndReturnsTrue()
    {
        var review = new Review { ReviewId = Guid.NewGuid() };
        var reviews = new List<Review> { review }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.DeleteAsync(review.ReviewId, CancellationToken.None);
        result.Should().BeTrue();
        _mockReviewRepo.Verify(r => r.Delete(review), Times.Once);
    }

    // ============================================================
    // CreateReplyAsync
    // ============================================================

    [Fact]
    public async Task CreateReplyAsync_ReviewNotFound_ReturnsNull()
    {
        var reviews = new List<Review>().AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.CreateReplyAsync(Guid.NewGuid(), new CreateReviewReplyDto(), CancellationToken.None);
        result.Should().BeNull();
    }

    // ============================================================
    // DeleteReplyAsync
    // ============================================================

    [Fact]
    public async Task DeleteReplyAsync_NotFound_ReturnsFalse()
    {
        var replies = new List<ReviewReply>().AsQueryable().BuildMock();
        _mockReplyRepo.Setup(r => r.Query()).Returns(replies);

        var result = await _service.DeleteReplyAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeFalse();
    }

    // ============================================================
    // AddImageAsync
    // ============================================================

    [Fact]
    public async Task AddImageAsync_ReviewNotFound_ReturnsNull()
    {
        var reviews = new List<Review>().AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.AddImageAsync(Guid.NewGuid(), new CreateReviewImageDto { ImageUrl = "url" }, CancellationToken.None);
        result.Should().BeNull();
    }

    // ============================================================
    // DeleteImageAsync
    // ============================================================

    [Fact]
    public async Task DeleteImageAsync_NotFound_ReturnsFalse()
    {
        var images = new List<ReviewImage>().AsQueryable().BuildMock();
        _mockImageRepo.Setup(r => r.Query()).Returns(images);

        var result = await _service.DeleteImageAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeFalse();
    }

    // ============================================================
    // GetVoteCountAsync / HasUserVotedAsync
    // ============================================================

    [Fact]
    public async Task GetVoteCountAsync_ReturnsCorrectCount()
    {
        var reviewId = Guid.NewGuid();
        var votes = new List<ReviewHelpfulVote>
        {
            new() { VoteId = Guid.NewGuid(), ReviewId = reviewId, UserId = Guid.NewGuid() },
            new() { VoteId = Guid.NewGuid(), ReviewId = reviewId, UserId = Guid.NewGuid() },
            new() { VoteId = Guid.NewGuid(), ReviewId = Guid.NewGuid(), UserId = Guid.NewGuid() }
        }.AsQueryable().BuildMock();
        _mockVoteRepo.Setup(r => r.Query()).Returns(votes);

        var result = await _service.GetVoteCountAsync(reviewId, CancellationToken.None);
        result.Should().Be(2);
    }

    [Fact]
    public async Task HasUserVotedAsync_HasVoted_ReturnsTrue()
    {
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votes = new List<ReviewHelpfulVote>
        {
            new() { VoteId = Guid.NewGuid(), ReviewId = reviewId, UserId = userId }
        }.AsQueryable().BuildMock();
        _mockVoteRepo.Setup(r => r.Query()).Returns(votes);

        var result = await _service.HasUserVotedAsync(reviewId, userId, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserVotedAsync_HasNotVoted_ReturnsFalse()
    {
        var votes = new List<ReviewHelpfulVote>().AsQueryable().BuildMock();
        _mockVoteRepo.Setup(r => r.Query()).Returns(votes);

        var result = await _service.HasUserVotedAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        result.Should().BeFalse();
    }

    // ============================================================
    // GetAllAsync / GetByIdAsync / GetByProductIdAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        var reviews = new List<Review>
        {
            new() { ReviewId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() },
            new() { ReviewId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddHours(-1), Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() }
        }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var reviewId = Guid.NewGuid();
        var reviews = new List<Review> { new() { ReviewId = reviewId, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() } }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.GetByIdAsync(reviewId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ReviewId.Should().Be(reviewId);
    }

    [Fact]
    public async Task GetByProductIdAsync_Found_ReturnsList()
    {
        var productId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            new() { ReviewId = Guid.NewGuid(), ProductId = productId, IsActive = 1, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() },
            new() { ReviewId = Guid.NewGuid(), ProductId = productId, IsActive = 1, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() },
            new() { ReviewId = Guid.NewGuid(), ProductId = Guid.NewGuid(), IsActive = 1, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() }
        }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);

        var result = await _service.GetByProductIdAsync(productId, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    // ============================================================
    // UpdateActiveAsync / UpdateReplyAsync
    // ============================================================

    [Fact]
    public async Task UpdateActiveAsync_ValidInput_UpdatesAndReturnsDto()
    {
        var reviewId = Guid.NewGuid();
        var review = new Review { ReviewId = reviewId, IsActive = 0, Product = new Product { Name = "P" }, User = new User { FullName = "U" }, HelpfulVotes = new List<ReviewHelpfulVote>() };
        var reviews = new List<Review> { review }.AsQueryable().BuildMock();
        _mockReviewRepo.Setup(r => r.Query()).Returns(reviews);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new UpdateReviewActiveDto { IsActive = 1 };
        var result = await _service.UpdateActiveAsync(reviewId, dto, CancellationToken.None);

        result.Should().NotBeNull();
        review.IsActive.Should().Be(1);
    }

    [Fact]
    public async Task UpdateReplyAsync_ValidInput_UpdatesAndReturnsDto()
    {
        var replyId = Guid.NewGuid();
        var reply = new ReviewReply { ReplyId = replyId, Content = "Old", User = new User { FullName = "U" } };
        var replies = new List<ReviewReply> { reply }.AsQueryable().BuildMock();
        _mockReplyRepo.Setup(r => r.Query()).Returns(replies);
        _mockUow.Setup(u => u.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new UpdateReviewReplyDto { Content = "New" };
        var result = await _service.UpdateReplyAsync(replyId, dto, CancellationToken.None);

        result.Should().NotBeNull();
        reply.Content.Should().Be("New");
    }

    // ============================================================
    // GetImagesByReviewIdAsync
    // ============================================================

    [Fact]
    public async Task GetImagesByReviewIdAsync_ReturnsList()
    {
        var reviewId = Guid.NewGuid();
        var images = new List<ReviewImage>
        {
            new() { ImageId = Guid.NewGuid(), ReviewId = reviewId },
            new() { ImageId = Guid.NewGuid(), ReviewId = reviewId }
        }.AsQueryable().BuildMock();
        _mockImageRepo.Setup(r => r.Query()).Returns(images);

        var result = await _service.GetImagesByReviewIdAsync(reviewId, CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
