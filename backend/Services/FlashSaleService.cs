using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.UnitOfWork;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using backend.Extensions;

namespace backend.Services;

public class FlashSaleService(IUnitOfWork uow, IMapper mapper, IActivityLogService activityLogService) : IFlashSaleService
{
	public async Task<FlashSaleDto> CreateAsync(
		CreateFlashSaleDto dto,
		CancellationToken cancellationToken = default)
	{
		ValidateFlashSaleWindow(dto.Title, dto.StartTime, dto.EndTime);

		// M5: chỉ một flash sale active tại cùng một thời điểm.
		if (dto.IsActive)
		{
			var hasOverlappingActiveSale = await uow.FlashSales.Query()
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
			StartTime = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc),
			EndTime = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc),
			IsActive = dto.IsActive,
			CreatedBy = dto.CreatedBy,
			CreatedAt = DateTime.UtcNow
		};

		uow.FlashSales.Insert(flashSale);
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
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

		var query = uow.FlashSales.Query()
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.AsQueryable();

		return await query
			.OrderByDescending(x => x.CreatedAt)
			.ProjectTo<FlashSaleDto>(mapper.ConfigurationProvider)
			.ToPagedResultAsync(page, pageSize, cancellationToken);
	}

	public async Task<FlashSaleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var flashSale = await uow.FlashSales.Query()
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.FirstOrDefaultAsync(x => x.FlashSaleId == id, cancellationToken)
			?? throw new NotFoundException("Flash sale not found");

		return mapper.Map<FlashSaleDto>(flashSale);
	}

	public async Task<FlashSaleDto> UpdateAsync(
		Guid id,
		UpdateFlashSaleDto dto,
		CancellationToken cancellationToken = default)
	{
		var flashSale = await uow.FlashSales.Query()
			.FirstOrDefaultAsync(x => x.FlashSaleId == id, cancellationToken)
			?? throw new NotFoundException("Flash sale not found");

		var nextTitle = string.IsNullOrWhiteSpace(dto.Title) ? flashSale.Title : dto.Title.Trim();
		var nextStartTime = dto.StartTime.HasValue
			? DateTime.SpecifyKind(dto.StartTime.Value, DateTimeKind.Utc)
			: flashSale.StartTime;
		var nextEndTime = dto.EndTime.HasValue
			? DateTime.SpecifyKind(dto.EndTime.Value, DateTimeKind.Utc)
			: flashSale.EndTime;
		var nextIsActive = dto.IsActive ?? flashSale.IsActive;

		ValidateFlashSaleWindow(nextTitle, nextStartTime, nextEndTime);

		if (nextIsActive)
		{
			var hasOverlappingActiveSale = await uow.FlashSales.Query()
				.AnyAsync(x => x.FlashSaleId != id
					&& x.IsActive
					&& x.StartTime < nextEndTime
					&& nextStartTime < x.EndTime,
					cancellationToken);

			if (hasOverlappingActiveSale)
				throw new BadRequestException("There is already an active flash sale in the selected time range");
		}

		var oldValue = new
		{
			flashSale.FlashSaleId,
			flashSale.Title,
			flashSale.StartTime,
			flashSale.EndTime,
			flashSale.IsActive
		};

		flashSale.Title = nextTitle;
		flashSale.StartTime = nextStartTime;
		flashSale.EndTime = nextEndTime;
		flashSale.IsActive = nextIsActive;

		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			flashSale.CreatedBy,
			"flashsale.update",
			"flash_sale",
			flashSale.FlashSaleId,
			oldValue,
			new
			{
				flashSale.FlashSaleId,
				flashSale.Title,
				flashSale.StartTime,
				flashSale.EndTime,
				flashSale.IsActive
			},
			cancellationToken);

		var updated = await uow.FlashSales.Query()
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.FirstAsync(x => x.FlashSaleId == id, cancellationToken);

		return mapper.Map<FlashSaleDto>(updated);
	}

	public async Task<FlashSaleDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var flashSale = await uow.FlashSales.Query()
			.Include(x => x.Items)
			.ThenInclude(i => i.Product)
			.FirstOrDefaultAsync(x => x.FlashSaleId == id, cancellationToken)
			?? throw new NotFoundException("Flash sale not found");

		if (!flashSale.IsActive)
			return mapper.Map<FlashSaleDto>(flashSale);

		var oldValue = new
		{
			flashSale.FlashSaleId,
			flashSale.Title,
			flashSale.StartTime,
			flashSale.EndTime,
			flashSale.IsActive
		};

		flashSale.IsActive = false;
		if (flashSale.EndTime > DateTime.UtcNow)
		{
			flashSale.EndTime = DateTime.UtcNow;
		}

		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			flashSale.CreatedBy,
			"flashsale.deactivate",
			"flash_sale",
			flashSale.FlashSaleId,
			oldValue,
			new
			{
				flashSale.FlashSaleId,
				flashSale.Title,
				flashSale.StartTime,
				flashSale.EndTime,
				flashSale.IsActive
			},
			cancellationToken);

		return mapper.Map<FlashSaleDto>(flashSale);
	}

	public async Task<FlashSaleDto?> GetActiveAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var active = await uow.FlashSales.Query()
			.Include(x => x.Items)
				.ThenInclude(i => i.Product)
					.ThenInclude(p => p.Images)
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
		var item = await uow.FlashSaleItems.Query()
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
		var flashSale = await uow.FlashSales.Query()
			.FirstOrDefaultAsync(x => x.FlashSaleId == flashSaleId, cancellationToken)
			?? throw new NotFoundException("Flash sale not found");

		var product = await uow.Products.Query()
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

		var existed = await uow.FlashSaleItems.Query()
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

		uow.FlashSaleItems.Insert(item);
		await uow.SaveAsync(cancellationToken);

		try
		{
			await activityLogService.LogIfHasUserAsync(
				flashSale.CreatedBy,
				"flashsale.item.add",
				"flash_sale_item",
				item.Id,
				null,
				item,
				cancellationToken);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[FLASHSALE_ITEM_LOG_ERROR] {ex.Message}");
		}

		return new FlashSaleItemDto
		{
			Id = item.Id,
			FlashSaleId = item.FlashSaleId,
			ProductId = item.ProductId,
			ProductName = product.Name,
			FlashPrice = item.FlashPrice,
			StockLimit = item.StockLimit,
			SoldCount = item.SoldCount,
			IsSoldOut = item.SoldCount >= item.StockLimit
		};
	}

	public async Task RemoveItemAsync(
		Guid flashSaleId,
		Guid productId,
		Guid? actorId,
		CancellationToken cancellationToken = default)
	{
		var item = await uow.FlashSaleItems.Query()
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

		uow.FlashSaleItems.Delete(item);
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			actorId,
			"flashsale.item.remove",
			"flash_sale_item",
			item.Id,
			oldValue,
			null,
			cancellationToken);
	}

	public async Task<bool> RecordPurchaseAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		// Tìm item đang trong đợt flash sale active
		var item = await uow.FlashSaleItems.Query()
			.Include(x => x.FlashSale)
			.Where(x => x.ProductId == productId
						&& x.FlashSale != null
						&& x.FlashSale.IsActive
						&& x.FlashSale.StartTime <= now
						&& x.FlashSale.EndTime >= now)
			.FirstOrDefaultAsync(cancellationToken);

		if (item == null) return false;

		// Sử dụng Raw SQL để tăng SoldCount với điều kiện để tránh overselling
		// PostgreSQL syntax
		var affectedRows = await uow.FlashSaleItems.Query()
			.Where(b => b.Id == item.Id && b.SoldCount + quantity <= b.StockLimit)
			.ExecuteUpdateAsync(s => s.SetProperty(b => b.SoldCount, b => b.SoldCount + quantity),
			cancellationToken);

		return affectedRows > 0;
	}

	private static void ValidateFlashSaleWindow(string? title, DateTime startTime, DateTime endTime)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new BadRequestException("Flash sale title is required");

		if (title.Trim().Length < 3 || title.Trim().Length > 100)
			throw new BadRequestException("Flash sale title must be between 3 and 100 characters");

		if (endTime <= startTime)
			throw new BadRequestException("EndTime must be greater than StartTime");
	}
}