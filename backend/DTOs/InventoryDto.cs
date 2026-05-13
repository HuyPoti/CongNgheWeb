using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class InventoryReceiptDto
{
    public Guid ReceiptId { get; set; }
    public Guid SupplierId { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Optional: Include basic supplier info
    public string? SupplierName { get; set; }
    
    public List<InventoryReceiptItemDto> Items { get; set; } = new();
}

public class InventoryReceiptItemDto
{
    public Guid ItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Optional: Include basic product info
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
}

public class CreateInventoryReceiptDto
{
    [Required]
    public Guid SupplierId { get; set; }
    
    public string? Notes { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "Receipt must contain at least one item.")]
    public List<CreateInventoryReceiptItemDto> Items { get; set; } = new();
}

public class CreateInventoryReceiptItemDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
    public decimal UnitPrice { get; set; }
}

public class InventoryTransactionDto
{
    public Guid TransactionId { get; set; }
    public Guid ProductId { get; set; }
    public int TransactionType { get; set; } // 1: Nhập kho, 2: Xuất bán, 3: Hoàn hàng, 4: Xuất hủy
    public Guid? ReferenceId { get; set; }
    public int QuantityChanged { get; set; }
    public int StockAfter { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? ProductName { get; set; }
}

public class AdjustStockDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public int QuantityChanged { get; set; } // Can be negative or positive
    
    [Required]
    public int TransactionType { get; set; } // 4: Xuất hủy, 5: Điều chỉnh (hoặc type tùy ý)
    
    [Required]
    public string Notes { get; set; } = string.Empty;
}

public class StockStatusDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int CurrentStock { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
