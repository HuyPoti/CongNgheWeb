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

public class BrandServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IBrandService _service;

    public BrandServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(UserProfile).Assembly); }); // Assuming UserProfile is enough or generic
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var uow = new backend.UnitOfWork.UnitOfWork(_context, mapper);
        _service = new BrandService(uow, mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "Brand 1", Slug = "brand-1" });
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "Brand 2", Slug = "brand-2" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBySlugAsync_Found_ReturnsBrand()
    {
        var slug = "test-brand";
        _context.Brands.Add(new Brand { BrandId = Guid.NewGuid(), Name = "Test Brand", Slug = slug });
        await _context.SaveChangesAsync();

        var result = await _service.GetBySlugAsync(slug, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Slug.Should().Be(slug);
    }
}
