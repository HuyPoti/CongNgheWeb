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
using System.Security.Cryptography;

namespace backend.Services;

public class AuthService(AppDbContext context, IConfiguration config, IMapper mapper, IEmailService emailService) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync( u => u.Email == dto.Email, cancellationToken);
        if (user == null)
            throw new Exception("Tài khoản không tồn tại.");
            
        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new Exception("Tài khoản này được đăng ký thông qua Google. Vui lòng sử dụng nút Đăng nhập Google.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Mật khẩu không chính xác.");
        
        if (!user.IsActive) throw new Exception("Tài khoản đã bị khóa.");

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            RefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken),
            User = mapper.Map<UserDto>(user)
        };
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

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            RefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken),
            User = mapper.Map<UserDto>(user)
        };
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
                    FullName = string.IsNullOrEmpty(payload.Name) ? payload.Email.Split('@')[0] : payload.Name,
                    PasswordHash = "",
                    Role = UserRole.customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                await context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                    throw new Exception("Email này đã được đăng ký bằng Mật khẩu. Vui lòng nhập Mật khẩu và Đăng nhập thông thường.");

                if (!user.IsActive)
                    throw new Exception("Tài khoản đã bị khóa.");
            }

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                RefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken),
                User = mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Lỗi xác thực Google: " + ex.Message);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null) throw new Exception("Email không tồn tại trong hệ thống");
        if (string.IsNullOrEmpty(user.PasswordHash)) throw new Exception("Tài khoản này dùng đăng nhập Google, không hỗ trợ đổi mật khẩu.");
        if (!user.IsActive) throw new Exception("Tài khoản đã bị khóa");

        var oldTokens = await context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var t in oldTokens)
        {
            t.IsUsed = true;
        }

        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
        };

        context.PasswordResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);
        var emailBody = $@"
            <h2>Đặt lại mật khẩu</h2>
            <p>Xin chào <strong>{user.FullName}</strong>,</p>
            <p>Mã OTP của bạn là:</p>
            <h1 style='color: #0066ff; letter-spacing: 8px; font-size: 36px;'>{otpCode}</h1>
            <p>Mã có hiệu lực trong <strong>10 phút</strong>.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        ";
       
        await emailService.SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu - TechShop", emailBody);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync( u =>u.Email == dto.Email, cancellationToken);
        if (user == null)   throw new Exception("Email không tồn tại");

        var token = await context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId 
                && !t.IsUsed 
                && t.OtpCode == dto.OtpCode 
                && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null) throw new Exception("Mã OTP không hợp lệ hoặc đã hết hạn");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        token.IsUsed = true;
        await context.SaveChangesAsync(cancellationToken);
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

    private async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken)
    {
        // Tạo token ngẫu nhiên
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenStr = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = tokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token sống 7 ngày
            IsRevoked = false
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return tokenStr;
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked, cancellationToken);

        if (storedToken == null)
            throw new Exception("Refresh token không hợp lệ.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            await context.SaveChangesAsync(cancellationToken);
            throw new Exception("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = storedToken.User;
        if (!user.IsActive)
            throw new Exception("Tài khoản đã bị khóa.");

        // Thu hồi token cũ
        storedToken.IsRevoked = true;

        // Cấp bộ token mới (rotation)
        var newRefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            RefreshToken = newRefreshToken,
            User = mapper.Map<UserDto>(user)
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked, cancellationToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}