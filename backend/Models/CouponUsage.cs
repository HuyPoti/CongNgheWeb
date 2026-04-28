using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("coupon_usages")]
public class CouponUsage
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("coupon_id")]
    public Guid CouponId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("used_at")]
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CouponId")]
    public Coupon? Coupon { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}
