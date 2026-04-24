// DTO = Data Transfer Object
// Mục đích: Kiểm soát dữ liệu GỬI VỀ cho frontend
// Tại sao không gửi thẳng Model? Vì Model có password_hash, không muốn lộ ra frontend!
using backend.Models;
namespace backend.DTOs;

public class UserDto
{
    public Guid  UserId { get; set; }
    public string Email {get ; set; } = string.Empty;
    public string FullName {get; set;} = string.Empty;
    public string? Phone {get; set;} = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role {get; set;} = UserRole.customer;
    public bool IsActive { get; set; }
    public bool HasPassword { get; set; }
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
}