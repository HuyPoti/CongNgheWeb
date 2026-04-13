using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("addresses")]
public class Address
{
    [Key]
    [Column("address_id")]
    public Guid AddressId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("recipient_name")]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("address_line")]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("province")]
    public string Province { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("district")]
    public string District { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("ward")]
    public string Ward { get; set; } = string.Empty;

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
