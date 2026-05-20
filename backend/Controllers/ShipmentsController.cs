using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;
using backend.Services;
using backend.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Tất cả endpoint đều cần đăng nhập, mỗi endpoint có quyền riêng
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;

    public ShipmentsController(IShipmentService service)
    {
        _service = service;
    }

    private Guid GetCurrentUserId()
    {
        var claimsInfo = string.Join("\n", User.Claims.Select(c => $"{c.Type}: {c.Value}"));
        System.IO.File.WriteAllText(System.IO.Path.Combine("d:\\Workspace\\Cong_Nghe_Web\\backend", "claims_debug.txt"), claimsInfo);

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                 ?? User.FindFirstValue("sub")
                 ?? User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
        
        return Guid.TryParse(idStr, out var guid) ? guid : Guid.Empty;
    }

    // POST: api/shipments — Chỉ Admin mới được tạo phiếu giao hàng (chọn hãng vận chuyển)
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> Create(
        [FromBody] CreateShipmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.CreateAsync(dto, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // PUT: api/shipments/{id} — Admin cập nhật mã vận đơn / hãng vận chuyển
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> Update(
        Guid id,
        [FromBody] UpdateShipmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.UpdateAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // GET: api/shipments/order/{orderId} — Admin, Staff, Warehouse đều được xem
    [HttpGet("order/{orderId}")]
    [Authorize(Roles = "admin,staff,warehouse")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.GetByOrderIdAsync(orderId, cancellationToken);
        if (shipment == null)
            return NotFound(ApiResponse.Fail("Shipment not found for this order"));

        return Ok(ApiResponse.Ok(shipment));
    }

    // PATCH: api/shipments/{id}/qc — CHỈ Nhân viên kho (warehouse) mới được xác nhận QC
    [HttpPatch("{id}/qc")]
    [Authorize(Roles = "warehouse")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> MarkQcPassed(
        Guid id,
        [FromBody] MarkQcDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.MarkQcPassedAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // PATCH: api/shipments/{id}/packed — CHỈ Nhân viên kho (warehouse) mới được đóng gói
    [HttpPatch("{id}/packed")]
    [Authorize(Roles = "warehouse")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> MarkPacked(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.MarkPackedAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }
}
