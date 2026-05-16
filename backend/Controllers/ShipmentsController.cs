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
[Authorize(Roles = "admin,staff")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;

    public ShipmentsController(IShipmentService service)
    {
        _service = service;
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        return Guid.TryParse(idStr, out var guid) ? guid : Guid.Empty;
    }

    // POST: api/shipments
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> Create(
        [FromBody] CreateShipmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.CreateAsync(dto, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // PUT: api/shipments/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> Update(
        Guid id,
        [FromBody] UpdateShipmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // GET: api/shipments/order/{orderId}
    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.GetByOrderIdAsync(orderId, cancellationToken);
        if (shipment == null)
            return NotFound(ApiResponse.Fail("Shipment not found for this order"));

        return Ok(ApiResponse.Ok(shipment));
    }

    // PATCH: api/shipments/{id}/qc
    [HttpPatch("{id}/qc")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> MarkQcPassed(
        Guid id,
        [FromBody] MarkQcDto dto,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.MarkQcPassedAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }

    // PATCH: api/shipments/{id}/packed
    [HttpPatch("{id}/packed")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> MarkPacked(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _service.MarkPackedAsync(id, GetCurrentUserId(), cancellationToken);
        return Ok(ApiResponse.Ok(shipment));
    }
}
