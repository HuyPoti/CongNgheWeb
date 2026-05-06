using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("inventory_transactions")]
public class InventoryTransaction
{
    [Key]
    [Column("transaction_id")]
    public Guid TransactionId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("transaction_type")]
    public int TransactionType { get; set; } // 1: Nhập kho, 2: Xuất bán, 3: Hoàn hàng, 4: Xuất hủy

    [Column("reference_id")]
    public Guid? ReferenceId { get; set; }

    [Required]
    [Column("quantity_changed")]
    public int QuantityChanged { get; set; }

    [Required]
    [Column("stock_after")]
    public int StockAfter { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    public User? Creator { get; set; }
}
