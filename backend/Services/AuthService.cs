using backend.UnitOfWork;
using backend.DTOs;
using backend.Exceptions;
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

public class AuthService(IUnitOfWork uow, IConfiguration config, IMapper mapper, IEmailService emailService, IEmailTemplateService emailTemplateService) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync( u => u.Email == dto.Email, cancellationToken);
        if (user == null)
            throw new NotFoundException("Tài khoản không tồn tại.");
            
        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BadRequestException("Tài khoản này được đăng ký thông qua Google. Vui lòng sử dụng nút Đăng nhập Google.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Mật khẩu không chính xác.");
        
        if (!user.IsActive) throw new UnauthorizedException("Tài khoản đã bị khóa.");

        if (!user.IsEmailVerified)
        {
            throw new UnauthorizedException("Tài khoản chưa xác thực email. Vui lòng kiểm tra email để lấy mã OTP.");
        }

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            RefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken),
            User = mapper.Map<UserDto>(user)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        if (await uow.Users.Query().AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new BadRequestException("Email đã tồn tại.");

        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Phone = dto.Phone,
            Role = UserRole.customer,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationOtp = otpCode,
            OtpExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        uow.Users.Insert(user);
        await uow.SaveAsync(cancellationToken);

        var emailBody = emailTemplateService.Render("verify-email", new Dictionary<string, string>
        {
            { "FullName", user.FullName },
            { "OtpCode", otpCode }
        });

        await emailService.SendEmailAsync(user.Email, "Xác nhận tài khoản - TechShop", emailBody);

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
            var user = await uow.Users.Query().FirstOrDefaultAsync( u => u.Email == payload.Email, cancellationToken);

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
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                uow.Users.Insert(user);
                await uow.SaveAsync(cancellationToken);
            }
            else
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                    throw new BadRequestException("Email này đã được đăng ký bằng Mật khẩu. Vui lòng nhập Mật khẩu và Đăng nhập thông thường.");

                if (!user.IsActive)
                    throw new UnauthorizedException("Tài khoản đã bị khóa.");
            }

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                RefreshToken = await GenerateRefreshTokenAsync(user, cancellationToken),
                User = mapper.Map<UserDto>(user)
            };
        }
        catch (UnauthorizedException)
        {
            throw; 
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UnauthorizedException("Lỗi xác thực Google: " + ex.Message);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null) throw new NotFoundException("Email không tồn tại trong hệ thống");
        if (string.IsNullOrEmpty(user.PasswordHash)) throw new BadRequestException("Tài khoản này dùng đăng nhập Google, không hỗ trợ đổi mật khẩu.");
        if (!user.IsActive) throw new UnauthorizedException("Tài khoản đã bị khóa");

        var oldTokens = await uow.PasswordResetTokens.Query()
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

        uow.PasswordResetTokens.Insert(resetToken);
        await uow.SaveAsync(cancellationToken);
        var emailBody = emailTemplateService.Render("reset-password", new Dictionary<string, string>
        {
            { "FullName", user.FullName },
            { "OtpCode", otpCode }
        });
       
        await emailService.SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu - TechShop", emailBody);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync( u =>u.Email == dto.Email, cancellationToken);
        if (user == null)   throw new NotFoundException("Email không tồn tại");

        var token = await uow.PasswordResetTokens.Query()
            .Where(t => t.UserId == user.UserId 
                && !t.IsUsed 
                && t.OtpCode == dto.OtpCode 
                && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null) throw new BadRequestException("Mã OTP không hợp lệ hoặc đã hết hạn");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        token.IsUsed = true;
        await uow.SaveAsync(cancellationToken);
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

        uow.RefreshTokens.Insert(refreshToken);
        await uow.SaveAsync(cancellationToken);

        return tokenStr;
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await uow.RefreshTokens.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked, cancellationToken);

        if (storedToken == null)
            throw new UnauthorizedException("Refresh token không hợp lệ.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            await uow.SaveAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = storedToken.User;
        if (!user.IsActive)
            throw new UnauthorizedException("Tài khoản đã bị khóa.");

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
        var storedToken = await uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked, cancellationToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await uow.SaveAsync(cancellationToken);
        }
    }

    public async Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null)
            throw new NotFoundException("Email không tồn tại.");

        if (user.IsEmailVerified)
            throw new BadRequestException("Email đã được xác nhận trước đó.");

        if (user.EmailVerificationOtp != dto.OtpCode)
            throw new BadRequestException("Mã OTP không chính xác.");

        if (user.OtpExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Mã OTP đã hết hạn. Vui lòng yêu cầu gửi lại.");

        user.IsEmailVerified = true;
        user.EmailVerificationOtp = null;  // Xóa OTP sau khi dùng
        user.OtpExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await uow.SaveAsync(cancellationToken);
    }

    public async Task ResendEmailAsync(ResendEmailDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null) throw new NotFoundException("Email không tồn tại.");
        if (!user.IsActive) throw new UnauthorizedException("Tài khoản đã bị khóa.");

        if (dto.Type == "forgot-password")
        {
            // Gửi lại OTP đặt lại mật khẩu
            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new BadRequestException("Tài khoản Google không hỗ trợ đặt lại mật khẩu.");

            var oldTokens = await uow.PasswordResetTokens.Query()
                .Where(t => t.UserId == user.UserId && !t.IsUsed)
                .ToListAsync(cancellationToken);
            foreach (var t in oldTokens) t.IsUsed = true;

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            uow.PasswordResetTokens.Insert(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            });
            await uow.SaveAsync(cancellationToken);

            var emailBody = emailTemplateService.Render("reset-password", new Dictionary<string, string>
            {
                { "FullName", user.FullName },
                { "OtpCode", otpCode }
            });
            await emailService.SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu (Gửi lại) - TechShop", emailBody);
        }
        else // mặc định: "verify"
        {
            if (user.IsEmailVerified) throw new BadRequestException("Email đã được xác nhận trước đó.");

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            user.EmailVerificationOtp = otpCode;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await uow.SaveAsync(cancellationToken);

            var emailBody = emailTemplateService.Render("verify-email", new Dictionary<string, string>
            {
                { "FullName", user.FullName },
                { "OtpCode", otpCode }
            });
            await emailService.SendEmailAsync(user.Email, "Mã xác nhận mới - TechShop", emailBody);
        }
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BadRequestException("Tài khoản Google không hỗ trợ đổi mật khẩu.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Mật khẩu hiện tại không chính xác.");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            throw new BadRequestException("Mật khẩu mới không được trùng với mật khẩu cũ.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await uow.SaveAsync(cancellationToken);
    }
}