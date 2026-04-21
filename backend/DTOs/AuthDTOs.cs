using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;

    [Required]
    public string Password {get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;

    [Required]
    public string Password {get; set; } = string.Empty;

    [Required]
    public string FullName {get; set; } = string.Empty;

    public string? Phone {get; set; }
}

public class GoogleLoginDto
{
    [Required]
    public string IdToken {get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;  // ← THÊM
    public UserDto User { get; set; } = null!;
}

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;
}

public class ResetPasswordDto{
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;


    [Required]
    [MaxLength(6)]
    public string OtpCode {get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword {get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}