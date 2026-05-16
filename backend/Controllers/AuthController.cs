using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using backend.Exceptions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authServices) : ControllerBase
{

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await authServices.LoginAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await authServices.RegisterAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleLogin([FromBody] GoogleLoginDto dto, CancellationToken cancellationToken)
    {
        var result = await authServices.GoogleLoginAsync(dto.IdToken, cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        await authServices.ForgotPasswordAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Mã OTP đã được gửi đến email của bạn." }));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        await authServices.ResetPasswordAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Mật khẩu đã được thay đổi thành công." }));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        var result = await authServices.RefreshTokenAsync(dto.RefreshToken, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        await authServices.LogoutAsync(dto.RefreshToken, ct);
        return Ok(ApiResponse.Ok(new { message = "Đã đăng xuất." }));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        await authServices.VerifyEmailAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Email đã được xác nhận thành công!" }));
    }

    [HttpPost("resend-email")]
    public async Task<ActionResult<ApiResponse<object>>> ResendEmail([FromBody] ResendEmailDto dto, CancellationToken cancellationToken)
    {
        await authServices.ResendEmailAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Mã OTP mới đã được gửi đến email của bạn." }));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ.");
        }

        await authServices.ChangePasswordAsync(userId, dto, cancellationToken);
        return Ok(ApiResponse.Ok(new { message = "Mật khẩu đã được thay đổi thành công." }));
    }

}