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
        services.AddAutoMapper(cfg => { 
            cfg.AddMaps(typeof(UserProfile).Assembly); 
            cfg.AddMaps(typeof(ProductProfile).Assembly);
            cfg.AddMaps(typeof(InventoryProfile).Assembly);
        });
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
        _context.Suppliers.Add(new Supplier { SupplierId = supplierId, Name = "Supplier 1", IsActive = true });
        _context.Users.Add(new User { UserId = adminId, FullName = "Admin", Email = "admin@a.com", IsActive = true, PasswordHash = "h" });
        await _context.SaveChangesAsync();

        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "Prod 1", Sku = "SKU1", Slug = "p1" });
        await _context.SaveChangesAsync();

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
        result.Status.Should().Be(1); // Draft
    }

    [Fact]
    public async Task CompleteReceiptAsync_ValidDraft_UpdatesStockAndCreatesTransactions()
    {
        var adminId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Sku = "S", Slug = "s", StockQuantity = 5 });
        _context.Suppliers.Add(new Supplier { SupplierId = supplierId, Name = "S" });
        
        var receiptId = Guid.NewGuid();
        var receipt = new InventoryReceipt { ReceiptId = receiptId, SupplierId = supplierId, Status = 1, CreatedBy = adminId, ReceiptCode = "REC-1" };
        _context.InventoryReceipts.Add(receipt);
        _context.InventoryReceiptItems.Add(new InventoryReceiptItem { ReceiptId = receiptId, ProductId = productId, Quantity = 10, UnitPrice = 100 });
        await _context.SaveChangesAsync();

        var result = await _service.CompleteReceiptAsync(receiptId, adminId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(2); // Completed
        
        var product = await _context.Products.FindAsync(productId);
        product!.StockQuantity.Should().Be(15); // 5 + 10

        var tx = await _context.InventoryTransactions.FirstOrDefaultAsync(t => t.ReferenceId == receiptId);
        tx.Should().NotBeNull();
        tx!.QuantityChanged.Should().Be(10);
    }

    [Fact]
    public async Task CancelReceiptAsync_CompletedReceipt_RollbacksStock()
    {
        var adminId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { ProductId = productId, Name = "P", Sku = "S", Slug = "s", StockQuantity = 20 };
        _context.Products.Add(product);
        
        var receiptId = Guid.NewGuid();
        var receipt = new InventoryReceipt { ReceiptId = receiptId, Status = 2, CreatedBy = adminId, ReceiptCode = "REC-1" };
        _context.InventoryReceipts.Add(receipt);
        _context.InventoryReceiptItems.Add(new InventoryReceiptItem { ReceiptId = receiptId, ProductId = productId, Quantity = 15, UnitPrice = 100 });
        await _context.SaveChangesAsync();

        var result = await _service.CancelReceiptAsync(receiptId, adminId, "Mistake", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(3); // Cancelled
        
        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.StockQuantity.Should().Be(5); // 20 - 15

        var tx = await _context.InventoryTransactions.OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();
        tx!.QuantityChanged.Should().Be(-15);
    }

    [Fact]
    public async Task AdjustStockAsync_IncrementsStock()
    {
        var adminId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Sku = "S", Slug = "s", StockQuantity = 10 });
        await _context.SaveChangesAsync();

        var dto = new AdjustStockDto { ProductId = productId, QuantityChanged = 5, TransactionType = 2, Notes = "Correction" };
        var result = await _service.AdjustStockAsync(dto, adminId, CancellationToken.None);

        result.Should().NotBeNull();
        result.StockAfter.Should().Be(15);
        
        var product = await _context.Products.FindAsync(productId);
        product!.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task GetStockStatusAsync_ReturnsAllProducts()
    {
        _context.Products.Add(new Product { ProductId = Guid.NewGuid(), Name = "A", Sku = "S1", Slug = "a", StockQuantity = 10 });
        _context.Products.Add(new Product { ProductId = Guid.NewGuid(), Name = "B", Sku = "S2", Slug = "b", StockQuantity = 20 });
        await _context.SaveChangesAsync();

        var result = await _service.GetStockStatusAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Any(r => r.ProductName == "A").Should().BeTrue();
    }

    [Fact]
    public async Task GetTransactionsAsync_ReturnsProductTransactions()
    {
        var productId = Guid.NewGuid();
        _context.Products.Add(new Product { ProductId = productId, Name = "P", Sku = "S", Slug = "s" });
        _context.InventoryTransactions.Add(new InventoryTransaction { TransactionId = Guid.NewGuid(), ProductId = productId, QuantityChanged = 10, CreatedAt = DateTime.UtcNow });
        _context.InventoryTransactions.Add(new InventoryTransaction { TransactionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), QuantityChanged = 5, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetTransactionsAsync(productId, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetReceiptByIdAsync_Found_ReturnsReceipt()
    {
        var receiptId = Guid.NewGuid();
        _context.InventoryReceipts.Add(new InventoryReceipt { ReceiptId = receiptId, ReceiptCode = "R1", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetReceiptByIdAsync(receiptId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ReceiptCode.Should().Be("R1");
    }

    [Fact]
    public async Task GetReceiptsAsync_ReturnsAll()
    {
        _context.InventoryReceipts.Add(new InventoryReceipt { ReceiptId = Guid.NewGuid(), ReceiptCode = "R1", CreatedAt = DateTime.UtcNow });
        _context.InventoryReceipts.Add(new InventoryReceipt { ReceiptId = Guid.NewGuid(), ReceiptCode = "R2", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetReceiptsAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
