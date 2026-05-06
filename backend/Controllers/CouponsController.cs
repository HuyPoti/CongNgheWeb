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
    public async Task<ActionResult<CouponDto>> Create([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CouponDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var res = await _couponService.GetAllAsync(page, pageSize, isActive, keyword, cancellationToken);
        return Ok(res);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CouponDto>> Update(Guid id, [FromBody] UpdateCouponDto dto, CancellationToken cancellationToken)
    {
        var updated = await _couponService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CouponDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var res = await _couponService.DeactivateAsync(id, cancellationToken);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<ActionResult<CouponValidationResultDto>> Validate([FromBody] CouponValidationRequestDto dto, CancellationToken cancellationToken)
    {
        var r = await _couponService.ValidateAsync(dto.Code, dto.TotalAmount, dto.UserId, cancellationToken);
        return Ok(r);
    }
}