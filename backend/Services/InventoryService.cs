using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class InventoryService(IUnitOfWork uow, IMapper mapper) : IInventoryService
{
    public async Task<InventoryReceiptDto> CreateReceiptAsync(CreateInventoryReceiptDto dto, Guid userId, CancellationToken cancellationToken)
    {
        var receiptId = Guid.NewGuid();
        
        var receipt = new InventoryReceipt
        {
            ReceiptId = receiptId,
            SupplierId = dto.SupplierId,
            ReceiptCode = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
            CreatedBy = userId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice)
        };

        uow.InventoryReceipts.Insert(receipt);

        foreach (var itemDto in dto.Items)
        {
            var product = await uow.Products.GetByIdAsync<Product>(itemDto.ProductId, cancellationToken);
            if (product == null) continue;

            // 1. Create Receipt Item
            var receiptItem = new InventoryReceiptItem
            {
                ItemId = Guid.NewGuid(),
                ReceiptId = receiptId,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TotalPrice = itemDto.Quantity * itemDto.UnitPrice
            };
            uow.InventoryReceiptItems.Insert(receiptItem);

            // 2. Update Product Stock
            product.StockQuantity += itemDto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            uow.Products.Update(product);

            // 3. Create Transaction Log
            var transaction = new InventoryTransaction
            {
                TransactionId = Guid.NewGuid(),
                ProductId = itemDto.ProductId,
                TransactionType = 1, // Nhập kho
                ReferenceId = receiptId,
                QuantityChanged = itemDto.Quantity,
                StockAfter = product.StockQuantity,
                CreatedBy = userId,
                Notes = $"Nhập kho từ phiếu {receipt.ReceiptCode}",
                CreatedAt = DateTime.UtcNow
            };
            uow.InventoryTransactions.Insert(transaction);
        }

        await uow.SaveAsync(cancellationToken);

        // Fetch back with details
        var savedReceipt = await uow.InventoryReceipts.Query()
            .Include(r => r.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);

        return mapper.Map<InventoryReceiptDto>(savedReceipt);
    }

    public async Task<List<InventoryReceiptDto>> GetReceiptsAsync(CancellationToken cancellationToken)
    {
        return await uow.InventoryReceipts.Query()
            .Include(r => r.Supplier)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectTo<InventoryReceiptDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryTransactionDto> AdjustStockAsync(AdjustStockDto dto, Guid userId, CancellationToken cancellationToken)
    {
        var product = await uow.Products.GetByIdAsync<Product>(dto.ProductId, cancellationToken);
        if (product == null) throw new Exception("Product not found");

        product.StockQuantity += dto.QuantityChanged;
        
        // Ensure stock doesn't go below 0 if not allowed, but usually adjusting handles negative values
        if (product.StockQuantity < 0) product.StockQuantity = 0;
        
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);

        var transaction = new InventoryTransaction
        {
            TransactionId = Guid.NewGuid(),
            ProductId = dto.ProductId,
            TransactionType = 4, // Xuất hủy / Điều chỉnh
            QuantityChanged = dto.QuantityChanged,
            StockAfter = product.StockQuantity,
            CreatedBy = userId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        uow.InventoryTransactions.Insert(transaction);

        await uow.SaveAsync(cancellationToken);

        var savedTx = await uow.InventoryTransactions.Query()
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId, cancellationToken);

        return mapper.Map<InventoryTransactionDto>(savedTx);
    }

    public async Task<List<InventoryTransactionDto>> GetTransactionsAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await uow.InventoryTransactions.Query()
            .Include(t => t.Product)
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.CreatedAt)
            .ProjectTo<InventoryTransactionDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
