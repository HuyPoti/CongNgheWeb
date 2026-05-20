using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,staff,warehouse")] // Admin, Staff & Warehouse
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(idStr)) throw new UnauthorizedAccessException("User not authenticated properly");
        return Guid.Parse(idStr);
    }

    [HttpGet("receipts")]
    public async Task<ActionResult<ApiResponse<List<InventoryReceiptDto>>>> GetReceipts(CancellationToken cancellationToken)
    {
        var receipts = await inventoryService.GetReceiptsAsync(cancellationToken);
        return Ok(ApiResponse.Ok(receipts));
    }

    [HttpGet("receipts/{id}")]
    public async Task<ActionResult<ApiResponse<InventoryReceiptDto>>> GetReceiptById(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await inventoryService.GetReceiptByIdAsync(id, cancellationToken);
        if (receipt == null) return NotFound(ApiResponse.Fail("Receipt not found"));
        return Ok(ApiResponse.Ok(receipt));
    }

    [HttpPost("receipts")]
    public async Task<ActionResult<ApiResponse<InventoryReceiptDto>>> CreateReceipt([FromBody] CreateInventoryReceiptDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var receipt = await inventoryService.CreateReceiptAsync(dto, userId, cancellationToken);
        return Ok(ApiResponse.Ok(receipt, "Receipt created successfully as Draft"));
    }

    [HttpPatch("receipts/{id}/complete")]
    public async Task<ActionResult<ApiResponse<InventoryReceiptDto>>> CompleteReceipt(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var receipt = await inventoryService.CompleteReceiptAsync(id, userId, cancellationToken);
        if (receipt == null) return NotFound(ApiResponse.Fail("Receipt not found"));
        return Ok(ApiResponse.Ok(receipt, "Receipt completed successfully"));
    }

    [HttpPatch("receipts/{id}/cancel")]
    public async Task<ActionResult<ApiResponse<InventoryReceiptDto>>> CancelReceipt(Guid id, [FromBody] CancelReceiptRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var receipt = await inventoryService.CancelReceiptAsync(id, userId, request.Reason, cancellationToken);
        if (receipt == null) return NotFound(ApiResponse.Fail("Receipt not found"));
        return Ok(ApiResponse.Ok(receipt, "Receipt cancelled successfully"));
    }

    [HttpGet("transactions/{productId}")]
    public async Task<ActionResult<ApiResponse<List<InventoryTransactionDto>>>> GetTransactions(Guid productId, CancellationToken cancellationToken)
    {
        var transactions = await inventoryService.GetTransactionsAsync(productId, cancellationToken);
        return Ok(ApiResponse.Ok(transactions));
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<ApiResponse<InventoryTransactionDto>>> AdjustStock([FromBody] AdjustStockDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var transaction = await inventoryService.AdjustStockAsync(dto, userId, cancellationToken);
        return Ok(ApiResponse.Ok(transaction, "Stock adjusted successfully"));
    }

    [HttpGet("stock-status")]
    public async Task<ActionResult<ApiResponse<List<StockStatusDto>>>> GetStockStatus(CancellationToken cancellationToken)
    {
        var status = await inventoryService.GetStockStatusAsync(cancellationToken);
        return Ok(ApiResponse.Ok(status));
    }
}


public class CancelReceiptRequest
{
    public string Reason { get; set; } = string.Empty;
}
