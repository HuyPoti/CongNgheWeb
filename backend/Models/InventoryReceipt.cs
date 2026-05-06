using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("inventory_receipts")]
public class InventoryReceipt
{
    [Key]
    [Column("receipt_id")]
    public Guid ReceiptId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("receipt_code")]
    public string ReceiptCode { get; set; } = string.Empty;

    [Column("supplier_id")]
    public Guid? SupplierId { get; set; }

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Required]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; } = 0;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1; // 1: Draft, 2: Completed, 3: Cancelled

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; }

    [ForeignKey("CreatedBy")]
    public User Creator { get; set; } = null!;

    public ICollection<InventoryReceiptItem> Items { get; set; } = new List<InventoryReceiptItem>();
}
