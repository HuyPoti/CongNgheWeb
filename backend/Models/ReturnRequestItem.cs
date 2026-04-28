using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("return_request_items")]
public class ReturnRequestItem
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("return_id")]
    public Guid ReturnId { get; set; }

    [Required]
    [Column("order_item_id")]
    public Guid OrderItemId { get; set; }

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("reason_detail")]
    public string? ReasonDetail { get; set; }

    [ForeignKey("ReturnId")]
    public ReturnRequest? ReturnRequest { get; set; }

    [ForeignKey("OrderItemId")]
    public OrderItem? OrderItem { get; set; }
}
