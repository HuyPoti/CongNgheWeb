using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("coupons")]
public class Coupon
{
    [Key]
    [Column("coupon_id")]
    public Guid CouponId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("discount_type")]
    public string DiscountType { get; set; } = string.Empty;

    [Required]
    [Column("discount_value")]
    public decimal DiscountValue { get; set; }

    [Column("min_order_amount")]
    public decimal MinOrderAmount { get; set; } = 0;

    [Column("max_discount")]
    public decimal? MaxDiscount { get; set; }

    [Column("usage_limit")]
    public int? UsageLimit { get; set; }

    [Column("used_count")]
    public int UsedCount { get; set; } = 0;

    [Column("per_user_limit")]
    public int PerUserLimit { get; set; } = 1;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CreatedBy")]
    public User? Creator { get; set; }
}
