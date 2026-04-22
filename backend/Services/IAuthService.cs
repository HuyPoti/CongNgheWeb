using backend.DTOs;

namespace backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);
    Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken);
    Task ResendVerificationAsync(ResendVerificationDto dto, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken);
}