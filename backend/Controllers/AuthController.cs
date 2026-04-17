using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;
using Google.Apis.Auth;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authServices;

    public AuthController(IAuthService authServices)
    {
        _authServices = authServices;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authServices.LoginAsync(dto, cancellationToken);
            if (response == null) return Unauthorized(new {message = "Email hoặc mật khẩu không đúng."});
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await _authServices.RegisterAsync(dto, cancellationToken)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await _authServices.GoogleLoginAsync(dto.IdToken, cancellationToken)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }   
}