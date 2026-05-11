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

public class SupplierServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ISupplierService _service;

    public SupplierServiceTests()
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

        var uow = new backend.UnitOfWork.UnitOfWork(_context, mapper);
        _service = new SupplierService(uow, mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        _context.Suppliers.Add(new Supplier { SupplierId = Guid.NewGuid(), Name = "S1" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
