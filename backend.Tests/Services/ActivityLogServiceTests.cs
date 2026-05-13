using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Tests.Services;

public class ActivityLogServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ActivityLogService _service;

    public ActivityLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _mockMapper = new Mock<IMapper>();
        
        // Setup Mapper mocks for LogAsync
        _mockMapper.Setup(m => m.Map<ActivityLogDto>(It.IsAny<ActivityLog>()))
            .Returns((ActivityLog src) => new ActivityLogDto
            {
                LogId = src.LogId,
                UserId = src.UserId,
                Action = src.Action,
                EntityType = src.EntityType,
                CreatedAt = src.CreatedAt,
                UserName = src.User?.FullName ?? "Unknown"
            });

        // Setup Mapper mocks for GetLogsAsync list mapping
        _mockMapper.Setup(m => m.Map<List<ActivityLogDto>>(It.IsAny<List<ActivityLog>>()))
            .Returns((List<ActivityLog> src) => src.Select(x => new ActivityLogDto
            {
                LogId = x.LogId,
                UserId = x.UserId,
                Action = x.Action,
                EntityType = x.EntityType,
                CreatedAt = x.CreatedAt,
                UserName = x.User?.FullName ?? "Unknown"
            }).ToList());

        _service = new ActivityLogService(_context, _mockMapper.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<User> CreateUserAsync(string name = "Test User")
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@test.com",
            FullName = name,
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // ============================================================
    // LogAsync
    // ============================================================

    [Fact]
    public async Task LogAsync_EmptyUserId_ThrowsBadRequestException()
    {
        var dto = new CreateActivityLogDto { UserId = Guid.Empty, Action = "Test" };
        var act = () => _service.LogAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("UserId is required*");
    }

    [Fact]
    public async Task LogAsync_EmptyAction_ThrowsBadRequestException()
    {
        var dto = new CreateActivityLogDto { UserId = Guid.NewGuid(), Action = "" };
        var act = () => _service.LogAsync(dto);
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Action is required");
    }

    [Fact]
    public async Task LogAsync_ValidInput_CreatesLogAndReturnsDto()
    {
        // Arrange
        var user = await CreateUserAsync();
        var dto = new CreateActivityLogDto
        {
            UserId = user.UserId,
            Action = "Test Action",
            EntityType = "Product",
            EntityId = Guid.NewGuid(),
            IpAddress = "127.0.0.1"
        };

        // Act
        var result = await _service.LogAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.UserId);
        result.Action.Should().Be("Test Action");
        result.UserName.Should().Be(user.FullName);

        var savedLog = await _context.ActivityLogs.FirstOrDefaultAsync(x => x.LogId == result.LogId);
        savedLog.Should().NotBeNull();
        savedLog!.Action.Should().Be("Test Action");
    }

    // ============================================================
    // GetLogsAsync
    // ============================================================

    [Fact]
    public async Task GetLogsAsync_NoFilters_ReturnsAllLogsPaginated()
    {
        // Arrange
        var user = await CreateUserAsync();
        for (int i = 0; i < 5; i++)
        {
            _context.ActivityLogs.Add(new ActivityLog { LogId = Guid.NewGuid(), UserId = user.UserId, Action = $"Action {i}" });
        }
        await _context.SaveChangesAsync();

        var query = new ActivityLogQueryDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetLogsAsync(query);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetLogsAsync_FilterByUserId_ReturnsOnlyUserLogs()
    {
        // Arrange
        var user1 = await CreateUserAsync("User 1");
        var user2 = await CreateUserAsync("User 2");
        _context.ActivityLogs.Add(new ActivityLog { LogId = Guid.NewGuid(), UserId = user1.UserId, Action = "A1" });
        _context.ActivityLogs.Add(new ActivityLog { LogId = Guid.NewGuid(), UserId = user2.UserId, Action = "A2" });
        await _context.SaveChangesAsync();

        var query = new ActivityLogQueryDto { UserId = user1.UserId };

        // Act
        var result = await _service.GetLogsAsync(query);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().UserId.Should().Be(user1.UserId);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetLogsAsync_FilterByDateRange_ReturnsCorrectLogs()
    {
        // Arrange
        var user = await CreateUserAsync();
        var now = DateTime.UtcNow;
        _context.ActivityLogs.Add(new ActivityLog { LogId = Guid.NewGuid(), UserId = user.UserId, Action = "Old", CreatedAt = now.AddDays(-10) });
        _context.ActivityLogs.Add(new ActivityLog { LogId = Guid.NewGuid(), UserId = user.UserId, Action = "New", CreatedAt = now.AddDays(-1) });
        await _context.SaveChangesAsync();

        var query = new ActivityLogQueryDto { FromDate = now.AddDays(-5) };

        // Act
        var result = await _service.GetLogsAsync(query);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Action.Should().Be("New");
    }
}
