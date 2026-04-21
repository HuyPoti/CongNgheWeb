using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("password_reset_tokens")]
public class PasswordResetToken
{
    [Key]
    [Column("id")]
    public Guid Id {get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId {get; set; }

    [Required]
    [MaxLength(6)]
    [Column("otp_code")]
    public string OtpCode {get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt {get; set; }

    [Column("is_used")]
    public bool IsUsed {get; set; } = false;

    [Column("created_at")]
    public DateTime CreateAt {get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public User User {get; set; } = null!;
}