using backend.DTOs;

namespace backend.Services;

public interface IInventoryService
{
    Task<InventoryReceiptDto> CreateReceiptAsync(CreateInventoryReceiptDto dto, Guid userId, CancellationToken cancellationToken);
    Task<InventoryReceiptDto?> GetReceiptByIdAsync(Guid receiptId, CancellationToken cancellationToken);
    Task<List<InventoryReceiptDto>> GetReceiptsAsync(CancellationToken cancellationToken);
    Task<InventoryReceiptDto?> CompleteReceiptAsync(Guid receiptId, Guid userId, CancellationToken cancellationToken);
    Task<InventoryReceiptDto?> CancelReceiptAsync(Guid receiptId, Guid userId, string reason, CancellationToken cancellationToken);
    Task<InventoryTransactionDto> AdjustStockAsync(AdjustStockDto dto, Guid userId, CancellationToken cancellationToken);
    Task<List<InventoryTransactionDto>> GetTransactionsAsync(Guid productId, CancellationToken cancellationToken);
    Task<List<StockStatusDto>> GetStockStatusAsync(CancellationToken cancellationToken);
}
