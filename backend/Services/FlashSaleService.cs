using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Services;

public interface IFlashSaleService
{
	Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto, CancellationToken cancellationToken = default);
	Task<PagedResult<FlashSaleDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
	Task<FlashSaleDto?> GetActiveAsync(CancellationToken cancellationToken = default);
	Task<decimal?> GetFlashPriceAsync(Guid productId, CancellationToken cancellationToken = default);
	Task<FlashSaleItemDto> AddItemAsync(Guid flashSaleId, CreateFlashSaleItemDto dto, CancellationToken cancellationToken = default);
	Task RemoveItemAsync(Guid flashSaleId, Guid productId, Guid? actorId, CancellationToken cancellationToken = default);
}

public class FlashSaleService(AppDbContext context, IMapper mapper, IActivityLogService activityLogService) : IFlashSaleService
{
	public async Task<FlashSaleDto> CreateAsync(
		CreateFlashSaleDto dto,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(dto.Title))
			throw new BadRequestException("Flash sale title is required");

		if (dto.EndTime <= dto.StartTime)
			throw new BadRequestException("EndTime must be greater than StartTime");

		// M5: chỉ một flash sale active tại cùng một thời điểm.
		if (dto.IsActive)
		{
			var hasOverlappingActiveSale = await context.FlashSales
				.AnyAsync(x => x.IsActive
					&& x.StartTime < dto.EndTime
					&& dto.StartTime < x.EndTime,
					cancellationToken);

			if (hasOverlappingActiveSale)
				throw new BadRequestException("There is already an active flash sale in the selected time range");
		}

		var flashSale = new FlashSale
		{
			FlashSaleId = Guid.NewGuid(),
			Title = dto.Title.Trim(),
			StartTime = dto.StartTime,
			EndTime = dto.EndTime,
			IsActive = dto.IsActive,
			CreatedBy = dto.CreatedBy,
			CreatedAt = DateTime.UtcNow
		};

		context.FlashSales.Add(flashSale);
		await context.SaveChangesAsync(cancellationToken);

		await LogIfHasUserAsync(
			dto.CreatedBy,
			"flashsale.create",
			"flash_sale",
			flashSale.FlashSaleId,
			null,
			flashSale,
			cancellationToken);

		return mapper.Map<FlashSaleDto>(flashSale);
	}

	public async Task<PagedResult<FlashSaleDto>> GetAllAsync(
		int page,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		page = page <= 0 ? 1 : page;
		pageSize = Math.Clamp(pageSize, 1, 50);

		var query = context.FlashSales
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.AsQueryable();

		var totalCount = await query.CountAsync(cancellationToken);
		var items = await query
			.OrderByDescending(x => x.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<FlashSaleDto>
		{
			Items = mapper.Map<List<FlashSaleDto>>(items),
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize
		};
	}

	public async Task<FlashSaleDto?> GetActiveAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var active = await context.FlashSales
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.Where(x => x.IsActive && x.StartTime <= now && x.EndTime >= now)
			.OrderBy(x => x.StartTime)
			.FirstOrDefaultAsync(cancellationToken);

		if (active == null)
			return null;

		active.Items = active.Items
			.Where(i => i.SoldCount < i.StockLimit)
			.ToList();

		return mapper.Map<FlashSaleDto>(active);
	}

	public async Task<decimal?> GetFlashPriceAsync(Guid productId, CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		var item = await context.FlashSaleItems
			.Include(x => x.FlashSale)
			.Where(x => x.ProductId == productId
						&& x.SoldCount < x.StockLimit
						&& x.FlashSale != null
						&& x.FlashSale.IsActive
						&& x.FlashSale.StartTime <= now
						&& x.FlashSale.EndTime >= now)
			.OrderBy(x => x.FlashPrice)
			.FirstOrDefaultAsync(cancellationToken);

		return item?.FlashPrice;
	}

	public async Task<FlashSaleItemDto> AddItemAsync(
		Guid flashSaleId,
		CreateFlashSaleItemDto dto,
		CancellationToken cancellationToken = default)
	{
		var flashSale = await context.FlashSales
			.FirstOrDefaultAsync(x => x.FlashSaleId == flashSaleId, cancellationToken)
			?? throw new NotFoundException("Flash sale not found");

		var product = await context.Products
			.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId, cancellationToken)
			?? throw new NotFoundException("Product not found");

		if (dto.StockLimit <= 0)
			throw new BadRequestException("StockLimit must be greater than zero");

		if (!product.SalePrice.HasValue)
			throw new BadRequestException("Product must have sale_price to join flash sale");

		if (product.SalePrice.Value <= 0 || product.RegularPrice <= 0)
			throw new BadRequestException("Product prices must be greater than zero");

		// M3: flash_price < sale_price < regular_price
		if (!(dto.FlashPrice < product.SalePrice.Value && product.SalePrice.Value < product.RegularPrice))
			throw new BadRequestException("Pricing rule violated: flash_price must be < sale_price < regular_price");

		// M7: SEO metadata length validation.
		if (!string.IsNullOrWhiteSpace(product.MetaTitle) && product.MetaTitle.Length > 60)
			throw new BadRequestException("metaTitle must not exceed 60 characters");

		if (!string.IsNullOrWhiteSpace(product.MetaDescription) && product.MetaDescription.Length > 160)
			throw new BadRequestException("metaDescription must not exceed 160 characters");

		var existed = await context.FlashSaleItems
			.AnyAsync(x => x.FlashSaleId == flashSaleId && x.ProductId == dto.ProductId, cancellationToken);

		if (existed)
			throw new BadRequestException("Product already exists in this flash sale");

		var item = new FlashSaleItem
		{
			Id = Guid.NewGuid(),
			FlashSaleId = flashSaleId,
			ProductId = dto.ProductId,
			FlashPrice = dto.FlashPrice,
			StockLimit = dto.StockLimit,
			SoldCount = 0
		};

		context.FlashSaleItems.Add(item);
		await context.SaveChangesAsync(cancellationToken);

		await LogIfHasUserAsync(
			flashSale.CreatedBy,
			"flashsale.item.add",
			"flash_sale_item",
			item.Id,
			null,
			item,
			cancellationToken);

		var saved = await context.FlashSaleItems
			.Include(x => x.Product)
			.FirstAsync(x => x.Id == item.Id, cancellationToken);

		return mapper.Map<FlashSaleItemDto>(saved);
	}

	public async Task RemoveItemAsync(
		Guid flashSaleId,
		Guid productId,
		Guid? actorId,
		CancellationToken cancellationToken = default)
	{
		var item = await context.FlashSaleItems
			.FirstOrDefaultAsync(x => x.FlashSaleId == flashSaleId && x.ProductId == productId, cancellationToken)
			?? throw new NotFoundException("Flash sale item not found");

		var oldValue = new
		{
			item.Id,
			item.FlashSaleId,
			item.ProductId,
			item.FlashPrice,
			item.StockLimit,
			item.SoldCount
		};

		context.FlashSaleItems.Remove(item);
		await context.SaveChangesAsync(cancellationToken);

		await LogIfHasUserAsync(
			actorId,
			"flashsale.item.remove",
			"flash_sale_item",
			item.Id,
			oldValue,
			null,
			cancellationToken);
	}

	private async Task LogIfHasUserAsync(
		Guid? userId,
		string action,
		string entityType,
		Guid entityId,
		object? oldValue,
		object? newValue,
		CancellationToken cancellationToken)
	{
		if (!userId.HasValue || userId.Value == Guid.Empty)
			return;

		await activityLogService.LogAsync(
			new CreateActivityLogDto
			{
				UserId = userId.Value,
				Action = action,
				EntityType = entityType,
				EntityId = entityId,
				OldValue = oldValue == null ? null : JsonSerializer.Serialize(oldValue),
				NewValue = newValue == null ? null : JsonSerializer.Serialize(newValue)
			},
			cancellationToken);
	}
}
