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

public class InventoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _service;

    public InventoryServiceTests()
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
        _service = new InventoryService(uow, mapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateReceiptAsync_Success_ReturnsReceipt()
    {
        var adminId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        _context.Suppliers.Add(new Supplier { SupplierId = supplierId, Name = "Supplier 1" });
        _context.Users.Add(new User { UserId = adminId, FullName = "Admin", Email = "admin@a.com", IsActive = true });
        await _context.SaveChangesAsync();

        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "Prod 1", Sku = "SKU1", Slug = "p1" });
        await _context.SaveChangesAsync();

        var dbProduct = await _context.Products.FindAsync(productId);
        dbProduct.Should().NotBeNull();

        var dto = new CreateInventoryReceiptDto
        {
            SupplierId = supplierId,
            Items = new List<CreateInventoryReceiptItemDto>
            {
                new() { ProductId = productId, Quantity = 10, UnitPrice = 100 }
            }
        };

        var result = await _service.CreateReceiptAsync(dto, adminId, CancellationToken.None);

        result.Should().NotBeNull();
        result.SupplierId.Should().Be(supplierId);
    }
}
