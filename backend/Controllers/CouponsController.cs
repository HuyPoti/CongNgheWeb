using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CouponDto>>> Create([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CouponDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var res = await _couponService.GetAllAsync(page, pageSize, isActive, keyword, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<PagedResult<CouponDto>>>> GetActive(CancellationToken cancellationToken)
    {
        var res = await _couponService.GetAllAsync(page: 1, pageSize: 50, isActive: true, keyword: null, cancellationToken);
        // Lọc bớt các mã coupon đã hết hạn
        var now = DateTime.UtcNow;
        res.Items = res.Items.Where(c => c.StartDate <= now && c.EndDate >= now).ToList();
        return Ok(ApiResponse.Ok(res));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var res = await _couponService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [AllowAnonymous]
    [HttpGet("code/{code}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var res = await _couponService.GetByCodeAsync(code, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> Update(Guid id, [FromBody] UpdateCouponDto dto, CancellationToken cancellationToken)
    {
        var updated = await _couponService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var res = await _couponService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok(res));
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<CouponValidationResultDto>>> Validate([FromBody] CouponValidationRequestDto dto, CancellationToken cancellationToken)
    {
        var r = await _couponService.ValidateAsync(dto.Code, dto.TotalAmount, dto.UserId, dto.Items, cancellationToken);
        return Ok(ApiResponse.Ok(r));
    }
}