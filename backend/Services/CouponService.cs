using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.UnitOfWork;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using backend.Extensions;
using backend.Constants;


namespace backend.Services;

public class CouponService(IUnitOfWork uow, IMapper mapper, IActivityLogService activityLogService, IFlashSaleService flashSaleService) : ICouponService
{
	public async Task<CouponDto> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
	{
		ValidateCouponInput(
			dto.DiscountType,
			dto.DiscountValue,
			dto.MinOrderAmount,
			dto.MaxDiscount,
			dto.UsageLimit,
			dto.PerUserLimit,
			dto.StartDate,
			dto.EndDate);

		if (string.IsNullOrWhiteSpace(dto.Code))
			throw new BadRequestException("Coupon code is required");

		var normalizedCode = dto.Code.Trim().ToUpperInvariant();
		var existed = await uow.Coupons.Query()
			.AnyAsync(x => x.Code == normalizedCode, cancellationToken);

		if (existed)
			throw new BadRequestException("Coupon code already exists");

		var coupon = new Coupon
		{
			CouponId = Guid.NewGuid(),
			Code = normalizedCode,
			Description = dto.Description,
			DiscountType = NormalizeDiscountType(dto.DiscountType),
			DiscountValue = dto.DiscountValue,
			MinOrderAmount = dto.MinOrderAmount,
			MaxDiscount = dto.MaxDiscount,
			UsageLimit = dto.UsageLimit,
			PerUserLimit = dto.PerUserLimit,
			StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
			EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
			IsActive = dto.IsActive,
			CreatedBy = dto.CreatedBy,
			CreatedAt = DateTime.UtcNow,
			UsedCount = 0
		};

		uow.Coupons.Insert(coupon);
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			dto.CreatedBy,
			"coupon.create",
			"coupon",
			coupon.CouponId,
			null,
			coupon,
			cancellationToken);

		return mapper.Map<CouponDto>(coupon);
	}

	public async Task<PagedResult<CouponDto>> GetAllAsync(
		int page,
		int pageSize,
		bool? isActive,
		string? keyword,
		CancellationToken cancellationToken = default)
	{
		page = page <= 0 ? 1 : page;
		pageSize = Math.Clamp(pageSize, 1, 50);

		var query = uow.Coupons.Query().AsQueryable();

		if (isActive.HasValue)
			query = query.Where(x => x.IsActive == isActive.Value);

		if (!string.IsNullOrWhiteSpace(keyword))
		{
			var kw = keyword.Trim();
			query = query.Where(x =>
				EF.Functions.ILike(x.Code, $"%{kw}%") ||
				(x.Description != null && EF.Functions.ILike(x.Description, $"%{kw}%")));
		}

		return await query
			.OrderByDescending(x => x.CreatedAt)
			.ProjectTo<CouponDto>(mapper.ConfigurationProvider)
			.ToPagedResultAsync(page, pageSize, cancellationToken);
	}

	public async Task<CouponValidationResultDto> ValidateAsync(
		string code,
		decimal totalAmount,
		Guid? userId,
		List<CouponValidationItemDto>? items = null,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return new CouponValidationResultDto
			{
				IsValid = false,
				Message = "Vui lòng nhập mã giảm giá",
				FinalAmount = totalAmount
			};
		}

		if (totalAmount <= 0)
		{
			return new CouponValidationResultDto
			{
				IsValid = false,
				Message = "Giá trị đơn hàng phải lớn hơn 0",
				FinalAmount = totalAmount
			};
		}

		var normalizedCode = code.Trim().ToUpperInvariant();
		var now = DateTime.UtcNow;
		var coupon = await uow.Coupons.Query()
			.FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

		if (coupon == null)
			return InvalidResult("Không tìm thấy mã giảm giá", totalAmount);

		if (!coupon.IsActive)
			return InvalidResult("Mã giảm giá đã bị ngưng hoạt động", totalAmount);

		if (now < coupon.StartDate || now > coupon.EndDate)
			return InvalidResult("Mã giảm giá đã hết hạn sử dụng", totalAmount);

		if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
			return InvalidResult("Mã giảm giá đã hết lượt sử dụng", totalAmount);

		if (userId.HasValue && userId.Value != Guid.Empty)
		{
			var usedByUser = await uow.CouponUsages.Query()
				.CountAsync(x => x.CouponId == coupon.CouponId && x.UserId == userId.Value, cancellationToken);
			if (usedByUser >= coupon.PerUserLimit)
				return InvalidResult("Bạn đã đạt giới hạn sử dụng mã giảm giá này", totalAmount);
		}

		decimal discountableAmount = totalAmount;
		if (items != null && items.Count > 0)
		{
			var productIds = items.Select(i => i.ProductId).Distinct().ToList();
			var products = await uow.Products.Query()
				.Where(p => productIds.Contains(p.ProductId))
				.ToDictionaryAsync(p => p.ProductId, cancellationToken);

			decimal flashSaleTotal = 0;
			foreach (var item in items)
			{
				if (products.TryGetValue(item.ProductId, out var product))
				{
					var flashPrice = await flashSaleService.GetFlashPriceAsync(product.ProductId, cancellationToken);
					if (flashPrice.HasValue)
					{
						flashSaleTotal += flashPrice.Value * item.Quantity;
					}
				}
			}

			if (flashSaleTotal > 0)
			{
				discountableAmount = totalAmount - flashSaleTotal;
				if (discountableAmount <= 0)
				{
					return InvalidResult("Mã giảm giá không áp dụng cho sản phẩm Flash Sale", totalAmount);
				}
			}
		}

		if (discountableAmount < coupon.MinOrderAmount)
			return InvalidResult($"Giá trị các sản phẩm thường phải đạt tối thiểu {coupon.MinOrderAmount:N0}đ để dùng mã này", totalAmount);

		var discountAmount = CalculateDiscount(coupon, discountableAmount);
		return new CouponValidationResultDto
		{
			IsValid = true,
			CouponId = coupon.CouponId,
			Code = coupon.Code,
			DiscountAmount = discountAmount,
			FinalAmount = Math.Max(totalAmount - discountAmount, 0),
			Message = "Mã giảm giá hợp lệ"
		};
	}

	public async Task<CouponUsageDto> ApplyAsync(
		Guid couponId,
		Guid orderId,
		Guid? userId,
		CancellationToken cancellationToken = default)
	{
		var coupon = await uow.Coupons.Query()
			.FirstOrDefaultAsync(x => x.CouponId == couponId, cancellationToken)
			?? throw new NotFoundException("Coupon not found");

		var order = await uow.Orders.Query()
			.Include(o => o.OrderItems)
			.FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken)
			?? throw new NotFoundException("Order not found");

		// M1: mỗi đơn chỉ được áp dụng 1 coupon.
		if (order.CouponId.HasValue)
			throw new BadRequestException("This order already has a coupon applied");

		var applyUserId = userId.HasValue && userId.Value != Guid.Empty
			? userId.Value
			: order.UserId;

		var validationItems = order.OrderItems.Select(oi => new CouponValidationItemDto
		{
			ProductId = oi.ProductId,
			Quantity = oi.Quantity
		}).ToList();

		var validation = await ValidateAsync(coupon.Code, order.TotalAmount, applyUserId, validationItems, cancellationToken);
		if (!validation.IsValid || !validation.CouponId.HasValue)
			throw new BadRequestException(validation.Message);

		var usage = new CouponUsage
		{
			Id = Guid.NewGuid(),
			CouponId = coupon.CouponId,
			UserId = applyUserId,
			OrderId = order.OrderId,
			DiscountAmount = validation.DiscountAmount,
			UsedAt = DateTime.UtcNow
		};

		coupon.UsedCount += 1;
		order.CouponId = coupon.CouponId;
		order.DiscountAmount = validation.DiscountAmount;
		order.UpdatedAt = DateTime.UtcNow;

		uow.CouponUsages.Insert(usage);
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			applyUserId,
			"coupon.apply",
			"order",
			order.OrderId,
			null,
			new { order.OrderId, coupon.CouponId, validation.DiscountAmount },
			cancellationToken);

		return mapper.Map<CouponUsageDto>(usage);
	}

	public async Task<CouponDto> DeactivateAsync(Guid couponId, CancellationToken cancellationToken = default)
	{
		var coupon = await uow.Coupons.Query()
			.FirstOrDefaultAsync(x => x.CouponId == couponId, cancellationToken)
			?? throw new NotFoundException("Coupon not found");

		if (!coupon.IsActive)
			return mapper.Map<CouponDto>(coupon);

		var old = new { coupon.CouponId, coupon.Code, coupon.IsActive };

		coupon.IsActive = false;
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(
			coupon.CreatedBy,
			"coupon.deactivate",
			"coupon",
			coupon.CouponId,
			old,
			new { coupon.CouponId, coupon.Code, coupon.IsActive },
			cancellationToken);

		return mapper.Map<CouponDto>(coupon);
	}

	public async Task<CouponDto?> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
	{
		var coupon = await uow.Coupons.Query().FirstOrDefaultAsync(x => x.CouponId == id, cancellationToken);
		if (coupon == null) throw new NotFoundException("Coupon not found");

		var oldVal = new { coupon.CouponId, coupon.Code, coupon.IsActive, coupon.DiscountValue, coupon.DiscountType };

		if (!string.IsNullOrWhiteSpace(dto.DiscountType))
			coupon.DiscountType = NormalizeDiscountType(dto.DiscountType!);
		if (dto.DiscountValue.HasValue)
			coupon.DiscountValue = dto.DiscountValue.Value;
		if (dto.MinOrderAmount.HasValue)
			coupon.MinOrderAmount = dto.MinOrderAmount.Value;
		if (dto.MaxDiscount.HasValue)
			coupon.MaxDiscount = dto.MaxDiscount.Value;
		if (dto.UsageLimit.HasValue)
			coupon.UsageLimit = dto.UsageLimit.Value;
		if (dto.PerUserLimit.HasValue)
			coupon.PerUserLimit = dto.PerUserLimit.Value;
		if (dto.StartDate.HasValue)
			coupon.StartDate = DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc);
		if (dto.EndDate.HasValue)
			coupon.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);
		if (dto.IsActive.HasValue)
			coupon.IsActive = dto.IsActive.Value;

		// Coupon model does not have UpdatedAt column; skip setting it here.
		await uow.SaveAsync(cancellationToken);

		await activityLogService.LogIfHasUserAsync(coupon.CreatedBy, "coupon.update", "coupon", coupon.CouponId, oldVal, coupon, cancellationToken);

		return mapper.Map<CouponDto>(coupon);
	}

	public async Task<CouponDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var coupon = await uow.Coupons.Query()
			.FirstOrDefaultAsync(x => x.CouponId == id, cancellationToken)
			?? throw new NotFoundException("Mã giảm giá không tồn tại");

		return mapper.Map<CouponDto>(coupon);
	}

	private static CouponValidationResultDto InvalidResult(string message, decimal totalAmount) =>
		new()
		{
			IsValid = false,
			Message = message,
			FinalAmount = totalAmount
		};

	private static string NormalizeDiscountType(string discountType)
	{
		var value = discountType.Trim().ToLowerInvariant();
		return value switch
		{
			"percent" or "percentage" => CouponDiscountType.Percentage,
			"fixed" or "amount" => CouponDiscountType.Fixed,
			_ => throw new BadRequestException($"DiscountType must be '{CouponDiscountType.Percentage}' or '{CouponDiscountType.Fixed}'")
		};
	}

	private static void ValidateCouponInput(
		string discountType,
		decimal discountValue,
		decimal minOrderAmount,
		decimal? maxDiscount,
		int? usageLimit,
		int perUserLimit,
		DateTime startDate,
		DateTime endDate)
	{
		var normalizedType = NormalizeDiscountType(discountType);

		if (discountValue <= 0)
			throw new BadRequestException("DiscountValue must be greater than zero");

		if (normalizedType == CouponDiscountType.Percentage && discountValue > 100)
			throw new BadRequestException("Percentage discount cannot exceed 100");

		if (minOrderAmount < 0)
			throw new BadRequestException("MinOrderAmount cannot be negative");

		if (maxDiscount.HasValue && maxDiscount.Value <= 0)
			throw new BadRequestException("MaxDiscount must be greater than zero");

		if (usageLimit.HasValue && usageLimit.Value <= 0)
			throw new BadRequestException("UsageLimit must be greater than zero");

		if (perUserLimit <= 0)
			throw new BadRequestException("PerUserLimit must be greater than zero");

		if (endDate <= startDate)
			throw new BadRequestException("EndDate must be greater than StartDate");
	}

	private static decimal CalculateDiscount(Coupon coupon, decimal totalAmount)
	{
		// M2: discount amount không được vượt maxDiscount với coupon dạng phần trăm.
		if (coupon.DiscountType == CouponDiscountType.Percentage)
		{
			var percentageDiscount = totalAmount * (coupon.DiscountValue / 100m);
			var cappedDiscount = coupon.MaxDiscount.HasValue
				? Math.Min(percentageDiscount, coupon.MaxDiscount.Value)
				: percentageDiscount;

			return Math.Min(cappedDiscount, totalAmount);
		}

		return Math.Min(coupon.DiscountValue, totalAmount);
	}
}
