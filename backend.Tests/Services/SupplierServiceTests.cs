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
        _context.Suppliers.Add(new Supplier { SupplierId = Guid.NewGuid(), Name = "S1", IsActive = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsSupplier()
    {
        var id = Guid.NewGuid();
        _context.Suppliers.Add(new Supplier { SupplierId = id, Name = "Test", IsActive = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCreatedSupplier()
    {
        var dto = new CreateSupplierDto { Name = "New", ContactName = "C", Email = "e@s.com", IsActive = true };
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("New");
        _context.Suppliers.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ValidInput_UpdatesSupplier()
    {
        var id = Guid.NewGuid();
        _context.Suppliers.Add(new Supplier { SupplierId = id, Name = "Old", IsActive = true });
        await _context.SaveChangesAsync();

        var dto = new UpdateSupplierDto { Name = "Updated" };
        var result = await _service.UpdateAsync(id, dto, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        var entity = await _context.Suppliers.FindAsync(id);
        entity!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_Found_MarksInactive()
    {
        var id = Guid.NewGuid();
        _context.Suppliers.Add(new Supplier { SupplierId = id, Name = "Del", IsActive = true });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(id, CancellationToken.None);

        result.Should().BeTrue();
        var entity = await _context.Suppliers.FindAsync(id);
        entity!.IsActive.Should().BeFalse();
    }
}
