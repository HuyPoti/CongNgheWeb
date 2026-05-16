using backend.DTOs;

namespace backend.Services;

public interface IFlashSaleService
{
	Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto, CancellationToken cancellationToken = default);
	Task<PagedResult<FlashSaleDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
	Task<FlashSaleDto?> GetActiveAsync(CancellationToken cancellationToken = default);
	Task<decimal?> GetFlashPriceAsync(Guid productId, CancellationToken cancellationToken = default);
	Task<FlashSaleItemDto> AddItemAsync(Guid flashSaleId, CreateFlashSaleItemDto dto, CancellationToken cancellationToken = default);
	Task RemoveItemAsync(Guid flashSaleId, Guid productId, Guid? actorId, CancellationToken cancellationToken = default);
	Task<bool> RecordPurchaseAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
