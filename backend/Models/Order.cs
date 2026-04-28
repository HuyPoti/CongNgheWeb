using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [Required]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    // 1: pending - 2: confirmed - 3: processing
    // 4: shipping - 5: delivered - 6: cancelled
    [Column("status")]
    public int Status { get; set; } = 1;

    [Required]
    [MaxLength(50)]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    // 1: unpaid - 2: paid - 3: refunded
    [Column("payment_status")]
    public int PaymentStatus { get; set; } = 1;

    [Column("shipping_fee")]
    public decimal ShippingFee { get; set; } = 0;

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column("cancelled_reason")]
    public string? CancelledReason { get; set; }

    [Column("cancelled_by")]
    public Guid? CancelledBy { get; set; }

    [Column("coupon_id")]
    public Guid? CouponId { get; set; }

    [Required]
    [Column("shipping_address_id")]
    public Guid ShippingAddressId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [ForeignKey("CancelledBy")]
    public User? CancelledByUser { get; set; }

    [ForeignKey("CouponId")]
    public Coupon? Coupon { get; set; }

    [ForeignKey("ShippingAddressId")]
    public Address Address { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
}
