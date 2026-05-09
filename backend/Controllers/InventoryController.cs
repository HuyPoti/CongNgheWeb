using System.Security.Claims;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "1,2")] // Admin & Employee
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    private Guid GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(idStr)) throw new UnauthorizedAccessException("User not authenticated properly");
        return Guid.Parse(idStr);
    }

    private IActionResult Success<T>(T data, string message = "Success")
    {
        return Ok(new { status = "success", data, message });
    }

    private IActionResult Error(string message, string details = "")
    {
        return BadRequest(new { status = "error", message, error = new { details } });
    }

    [HttpGet("receipts")]
    public async Task<IActionResult> GetReceipts(CancellationToken cancellationToken)
    {
        try
        {
            var receipts = await inventoryService.GetReceiptsAsync(cancellationToken);
            return Success(receipts);
        }
        catch (Exception ex)
        {
            return Error("Failed to get receipts", ex.Message);
        }
    }

    [HttpGet("receipts/{id}")]
    public async Task<IActionResult> GetReceiptById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var receipt = await inventoryService.GetReceiptByIdAsync(id, cancellationToken);
            if (receipt == null) return NotFound(new { status = "error", message = "Receipt not found" });
            return Success(receipt);
        }
        catch (Exception ex)
        {
            return Error("Failed to get receipt", ex.Message);
        }
    }

    [HttpPost("receipts")]
    public async Task<IActionResult> CreateReceipt([FromBody] CreateInventoryReceiptDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        try
        {
            var userId = GetUserId();
            var receipt = await inventoryService.CreateReceiptAsync(dto, userId, cancellationToken);
            return Success(receipt, "Receipt created successfully as Draft");
        }
        catch (Exception ex)
        {
            return Error("Failed to create receipt", ex.Message);
        }
    }

    [HttpPatch("receipts/{id}/complete")]
    public async Task<IActionResult> CompleteReceipt(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var receipt = await inventoryService.CompleteReceiptAsync(id, userId, cancellationToken);
            if (receipt == null) return NotFound(new { status = "error", message = "Receipt not found" });
            return Success(receipt, "Receipt completed successfully");
        }
        catch (Exception ex)
        {
            return Error("Failed to complete receipt", ex.Message);
        }
    }

    [HttpPatch("receipts/{id}/cancel")]
    public async Task<IActionResult> CancelReceipt(Guid id, [FromBody] CancelReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var receipt = await inventoryService.CancelReceiptAsync(id, userId, request.Reason, cancellationToken);
            if (receipt == null) return NotFound(new { status = "error", message = "Receipt not found" });
            return Success(receipt, "Receipt cancelled successfully");
        }
        catch (Exception ex)
        {
            return Error("Failed to cancel receipt", ex.Message);
        }
    }

    [HttpGet("transactions/{productId}")]
    public async Task<IActionResult> GetTransactions(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await inventoryService.GetTransactionsAsync(productId, cancellationToken);
            return Success(transactions);
        }
        catch (Exception ex)
        {
            return Error("Failed to get transactions", ex.Message);
        }
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        try
        {
            var userId = GetUserId();
            var transaction = await inventoryService.AdjustStockAsync(dto, userId, cancellationToken);
            return Success(transaction, "Stock adjusted successfully");
        }
        catch (Exception ex)
        {
            return Error("Failed to adjust stock", ex.Message);
        }
    }

    [HttpGet("stock-status")]
    public async Task<IActionResult> GetStockStatus(CancellationToken cancellationToken)
    {
        try
        {
            var status = await inventoryService.GetStockStatusAsync(cancellationToken);
            return Success(status);
        }
        catch (Exception ex)
        {
            return Error("Failed to get stock status", ex.Message);
        }
    }
}

public class CancelReceiptRequest
{
    public string Reason { get; set; } = string.Empty;
}
