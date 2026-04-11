using System.ComponentModel.DataAnnotations;
using backend.Models;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email {get; set; } = string.Empty;

    [Required(ErrorMessage = "Mât khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string Password {get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    public string FullName {get; set; } = string.Empty;

    public string? Phone {get; set; }

    public UserRole Role {get; set; } = UserRole.customer;
}