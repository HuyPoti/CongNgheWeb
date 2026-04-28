using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("flash_sale_items")]
public class FlashSaleItem
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("flash_sale_id")]
    public Guid FlashSaleId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("flash_price")]
    public decimal FlashPrice { get; set; }

    [Required]
    [Column("stock_limit")]
    public int StockLimit { get; set; }

    [Column("sold_count")]
    public int SoldCount { get; set; } = 0;

    [ForeignKey("FlashSaleId")]
    public FlashSale? FlashSale { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}
