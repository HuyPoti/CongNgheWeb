using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.UnitOfWork;
using backend.MapperProfiles;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace backend.Tests.Services;

public class ReturnRequestServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IReturnRequestService _service;

    public ReturnRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(UserProfile).Assembly); });
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        _service = new ReturnRequestService(_context, mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { UserId = userId, FullName = "User 1", Email = "u@u.com" });
        _context.Orders.Add(new Order { OrderId = orderId1 });
        _context.Orders.Add(new Order { OrderId = orderId2 });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = Guid.NewGuid(), OrderId = orderId1, UserId = userId, Reason = "Reason 1" });
        _context.ReturnRequests.Add(new ReturnRequest { ReturnId = Guid.NewGuid(), OrderId = orderId2, UserId = userId, Reason = "Reason 2" });
        await _context.SaveChangesAsync();

        var count = await _context.ReturnRequests.CountAsync();
        count.Should().Be(2);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }
}
