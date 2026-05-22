using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.MapperProfiles;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.Services;

public class ProfileServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProfileService _service;
    private readonly IMapper _mapper;

    public ProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(UserProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, _mapper);
        _service = new ProfileService(uow, _mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetProfileAsync_UserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "John Doe", Email = "john@example.com", PasswordHash = "hash" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfileAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetProfileAsync_UserNotFound_ReturnsNull()
    {
        // Act
        var result = await _service.GetProfileAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_UserExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, FullName = "Old Name", Email = "john@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UpdateProfileDto { FullName = "New Name", Phone = "123456789" };

        // Act
        var result = await _service.UpdateProfileAsync(userId, dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("New Name");
        
        var updatedUser = await _context.Users.FindAsync(userId);
        updatedUser!.FullName.Should().Be("New Name");
        updatedUser.Phone.Should().Be("123456789");
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNull()
    {
        // Act
        var result = await _service.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileDto(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
