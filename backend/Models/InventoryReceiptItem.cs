using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("inventory_receipt_items")]
public class InventoryReceiptItem
{
    [Key]
    [Column("item_id")]
    public Guid ItemId { get; set; }

    [Required]
    [Column("receipt_id")]
    public Guid ReceiptId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column("total_price")]
    public decimal TotalPrice { get; set; }

    // Navigation
    [ForeignKey("ReceiptId")]
    public InventoryReceipt Receipt { get; set; } = null!;

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}
