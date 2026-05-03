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
        return Guid.Parse(idStr!);
    }

    [HttpGet("receipts")]
    public async Task<IActionResult> GetReceipts(CancellationToken cancellationToken)
    {
        var receipts = await inventoryService.GetReceiptsAsync(cancellationToken);
        return Ok(receipts);
    }

    [HttpPost("receipts")]
    public async Task<IActionResult> CreateReceipt([FromBody] CreateInventoryReceiptDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = GetUserId();
        var receipt = await inventoryService.CreateReceiptAsync(dto, userId, cancellationToken);
        return Ok(receipt);
    }

    [HttpGet("transactions/{productId}")]
    public async Task<IActionResult> GetTransactions(Guid productId, CancellationToken cancellationToken)
    {
        var transactions = await inventoryService.GetTransactionsAsync(productId, cancellationToken);
        return Ok(transactions);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = GetUserId();
        try
        {
            var transaction = await inventoryService.AdjustStockAsync(dto, userId, cancellationToken);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
