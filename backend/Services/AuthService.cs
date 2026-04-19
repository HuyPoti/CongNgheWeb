using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;

namespace backend.Services;

public class AuthService(AppDbContext context, IConfiguration config, IMapper mapper) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync( u => u.Email == dto.Email, cancellationToken);
        if (user == null)
            throw new Exception("Tài khoản không tồn tại.");
            
        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Mật khẩu không chính xác.");
        
        if (!user.IsActive) throw new Exception("Tài khoản đã bị khóa.");

        return new AuthResponseDto { Token = GenerateJwtToken(user), User = mapper.Map<UserDto>(user)};
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new Exception("Email đã tồn tại.");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Phone = dto.Phone,
            Role = UserRole.customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto { Token = GenerateJwtToken(user), User = mapper.Map<UserDto>(user)};
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> {config["GoogleAuth:ClientId"]!}
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            var user = await context.Users.FirstOrDefaultAsync( u => u.Email == payload.Email, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = payload.Email,
                    FullName = payload.Name,
                    PasswordHash = "",
                    Role = UserRole.customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                await context.SaveChangesAsync(cancellationToken);
            }
            else if (!user.IsActive)
            {
                throw new Exception("Tài khoản đã bị khóa.");
            }

            return new AuthResponseDto { Token = GenerateJwtToken(user), User = mapper.Map<UserDto>(user)};
        }
        catch (Exception ex)
        {
            throw new Exception("Lỗi xác thực Google: " + ex.Message);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpireMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}