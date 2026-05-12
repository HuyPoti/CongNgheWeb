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
            Status = 1, // 1: Draft
            CreatedAt = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice)
        };

        uow.InventoryReceipts.Insert(receipt);

        foreach (var itemDto in dto.Items)
        {
            var product = await uow.Products.Query().FirstOrDefaultAsync(p => p.ProductId == itemDto.ProductId, cancellationToken);
            if (product == null) throw new Exception($"Product with id {itemDto.ProductId} not found");

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
            
            // Note: We DO NOT update product stock or create transactions here because it's Draft.
        }

        await uow.SaveAsync(cancellationToken);

        return await GetReceiptByIdAsync(receiptId, cancellationToken) ?? throw new Exception("Failed to retrieve saved receipt");
    }

    public async Task<InventoryReceiptDto?> GetReceiptByIdAsync(Guid receiptId, CancellationToken cancellationToken)
    {
        var receipt = await uow.InventoryReceipts.Query()
            .Include(r => r.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);

        return receipt == null ? null : mapper.Map<InventoryReceiptDto>(receipt);
    }

    public async Task<List<InventoryReceiptDto>> GetReceiptsAsync(CancellationToken cancellationToken)
    {
        return await uow.InventoryReceipts.Query()
            .Include(r => r.Supplier)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectTo<InventoryReceiptDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryReceiptDto?> CompleteReceiptAsync(Guid receiptId, Guid userId, CancellationToken cancellationToken)
    {
        var receipt = await uow.InventoryReceipts.Query()
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);

        if (receipt == null) return null;
        if (receipt.Status != 1) throw new Exception("Only draft receipts can be completed");

        receipt.Status = 2; // 2: Completed
        receipt.UpdatedAt = DateTime.UtcNow;
        uow.InventoryReceipts.Update(receipt);

        foreach (var item in receipt.Items)
        {
            var product = item.Product;
            product.StockQuantity += item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            uow.Products.Update(product);

            var transaction = new InventoryTransaction
            {
                TransactionId = Guid.NewGuid(),
                ProductId = item.ProductId,
                TransactionType = 1, // 1: Nhập kho
                ReferenceId = receiptId,
                QuantityChanged = item.Quantity,
                StockAfter = product.StockQuantity,
                CreatedBy = userId,
                Notes = $"Nhập kho từ phiếu {receipt.ReceiptCode}",
                CreatedAt = DateTime.UtcNow
            };
            uow.InventoryTransactions.Insert(transaction);
        }

        await uow.SaveAsync(cancellationToken);
        return await GetReceiptByIdAsync(receiptId, cancellationToken);
    }

    public async Task<InventoryReceiptDto?> CancelReceiptAsync(Guid receiptId, Guid userId, string reason, CancellationToken cancellationToken)
    {
        var receipt = await uow.InventoryReceipts.Query()
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReceiptId == receiptId, cancellationToken);

        if (receipt == null) return null;
        if (receipt.Status == 3) throw new Exception("Receipt is already cancelled");

        var previousStatus = receipt.Status;
        receipt.Status = 3; // 3: Cancelled
        receipt.UpdatedAt = DateTime.UtcNow;
        receipt.Notes = string.IsNullOrEmpty(receipt.Notes) ? $"Lý do hủy: {reason}" : $"{receipt.Notes}\nLý do hủy: {reason}";
        
        uow.InventoryReceipts.Update(receipt);

        // Nếu phiếu đã Completed (2), ta phải rollback số lượng (Xuất trả lại)
        if (previousStatus == 2)
        {
            foreach (var item in receipt.Items)
            {
                var product = item.Product;
                product.StockQuantity -= item.Quantity;
                if (product.StockQuantity < 0) product.StockQuantity = 0; // Prevent negative
                product.UpdatedAt = DateTime.UtcNow;
                uow.Products.Update(product);

                var transaction = new InventoryTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    TransactionType = 4, // 4: Xuất hủy/Điều chỉnh do hủy phiếu
                    ReferenceId = receiptId,
                    QuantityChanged = -item.Quantity,
                    StockAfter = product.StockQuantity,
                    CreatedBy = userId,
                    Notes = $"Rollback nhập kho (Hủy phiếu {receipt.ReceiptCode}): {reason}",
                    CreatedAt = DateTime.UtcNow
                };
                uow.InventoryTransactions.Insert(transaction);
            }
        }

        await uow.SaveAsync(cancellationToken);
        return await GetReceiptByIdAsync(receiptId, cancellationToken);
    }

    public async Task<InventoryTransactionDto> AdjustStockAsync(AdjustStockDto dto, Guid userId, CancellationToken cancellationToken)
    {
        var product = await uow.Products.Query().FirstOrDefaultAsync(p => p.ProductId == dto.ProductId, cancellationToken);
        if (product == null) throw new Exception("Product not found");

        product.StockQuantity += dto.QuantityChanged;
        if (product.StockQuantity < 0) product.StockQuantity = 0;
        
        product.UpdatedAt = DateTime.UtcNow;
        uow.Products.Update(product);

        var transaction = new InventoryTransaction
        {
            TransactionId = Guid.NewGuid(),
            ProductId = dto.ProductId,
            TransactionType = dto.TransactionType, // Dynamic type
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

    public async Task<List<StockStatusDto>> GetStockStatusAsync(CancellationToken cancellationToken)
    {
        return await uow.Products.Query()
            .OrderBy(p => p.Name)
            .Select(p => new StockStatusDto
            {
                ProductId = p.ProductId,
                ProductName = p.Name,
                ProductSku = p.Sku,
                CurrentStock = p.StockQuantity,
                LastUpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
