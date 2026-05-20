using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,staff,warehouse")] // Admin, Staff & Warehouse
public class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var suppliers = await supplierService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse.Ok(suppliers));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await supplierService.GetByIdAsync(id, cancellationToken);
        if (supplier == null) return NotFound(ApiResponse.Fail("Không tìm thấy nhà cung cấp"));
        return Ok(ApiResponse.Ok(supplier));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create([FromBody] CreateSupplierDto dto, CancellationToken cancellationToken)
    {
        var created = await supplierService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.SupplierId }, ApiResponse.Ok(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken cancellationToken)
    {
        var updated = await supplierService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound(ApiResponse.Fail("Không tìm thấy nhà cung cấp"));
        return Ok(ApiResponse.Ok(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await supplierService.DeleteAsync(id, cancellationToken);
        if (!success) return NotFound(ApiResponse.Fail("Không tìm thấy nhà cung cấp"));
        return Ok(ApiResponse.Ok(new { message = "Suppler deleted successfully" }));
    }
}
