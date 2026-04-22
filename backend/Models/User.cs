// File này ĐỊNH NGHĨA cấu trúc dữ liệu User
// Nó "ánh xạ" (map) 1-1 với bảng "users" trong PostgreSQL
// Mỗi property trong class = 1 cột trong bảng

using System.ComponentModel.DataAnnotations;        // ← Thư viện validation
using System.ComponentModel.DataAnnotations.Schema;  // ← Thư viện ánh xạ bảng

namespace backend.Models;  // ← Namespace = "địa chỉ" của class, giúp tổ chức code

[Table("users")]  // ← Nói cho EF Core biết: class này map với bảng tên "users"
public class User
{
    [Key]                           // ← Đánh dấu đây là Primary Key
    [Column("user_id")]             // ← Map với cột "user_id" trong DB
    public Guid UserId { get; set; } // ← Property C# dùng PascalCase

    [Required]                          // ← Bắt buộc không được null
    [MaxLength(255)]                    // ← Tối đa 255 ký tự
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }   // ← Dấu ? nghĩa là nullable (cho phép null)

    [Column("role")]
    public UserRole Role { get; set; } = UserRole.customer;  // ← Giá trị mặc định

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_email_verified")]
    public bool IsEmailVerified {get; set;} = false;

    [Column("email_verification_otp")]
    [MaxLength(6)]
    public string? EmailVerificationOtp {get; set; }

    [Column("otp_expires_at")]
    public DateTime? OtpExpiresAt {get; set;}

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
