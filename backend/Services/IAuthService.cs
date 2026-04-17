using backend.DTOs;

namespace backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken);
}