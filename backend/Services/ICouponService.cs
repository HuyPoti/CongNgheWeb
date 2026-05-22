using System.Collections.Generic;
using backend.DTOs;

namespace backend.Services;

public interface ICouponService
{
	Task<CouponDto> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);
	Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize, bool? isActive, string? keyword, CancellationToken cancellationToken = default);
	Task<CouponValidationResultDto> ValidateAsync(string code, decimal totalAmount, Guid? userId, List<CouponValidationItemDto>? items = null, CancellationToken cancellationToken = default);
	Task<CouponUsageDto> ApplyAsync(Guid couponId, Guid orderId, Guid? userId, CancellationToken cancellationToken = default);
	Task<CouponDto> DeactivateAsync(Guid couponId, CancellationToken cancellationToken = default);
	Task<CouponDto?> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default);
	Task<CouponDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<CouponDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
