using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
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
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authServices.LoginAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new {message = ex.Message});
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await authServices.RegisterAsync(dto, cancellationToken)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await authServices.GoogleLoginAsync(dto.IdToken, cancellationToken)); }
        catch (Exception ex) 
        { 
            var msg = ex.Message;
            if (ex.InnerException != null) msg += " | Inner: " + ex.InnerException.Message;
            return BadRequest(new { message = msg }); 
        }
    }   

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await authServices.ForgotPasswordAsync(dto, cancellationToken);
            return Ok(new {message = "Mã OTP đã được gửi đến email của bạn." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        try{
            await authServices.ResetPasswordAsync(dto, cancellationToken);
            return Ok(new {message = "Mật khẩu đã được thay đổi thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        try { return Ok(await authServices.RefreshTokenAsync(dto.RefreshToken, ct)); }
        catch (Exception ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        await authServices.LogoutAsync(dto.RefreshToken, ct);
        return Ok(new { message = "Đã đăng xuất." });
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await authServices.VerifyEmailAsync(dto, cancellationToken);
            return Ok(new { message = "Email đã được xác nhận thành công!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }                                                       

    [HttpPost("resend-verification")]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await authServices.ResendVerificationAsync(dto, cancellationToken);
            return Ok(new { message = "Mã OTP mới đã được gửi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            await authServices.ChangePasswordAsync(userId, dto, cancellationToken);
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}