using AutoMapper;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace backend.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("TestSecretKeyThatIsLongEnough1234567890123456");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfig.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");
        _mockConfig.Setup(c => c["GoogleAuth:ClientId"]).Returns("test-client-id");

        _mockMapper = new Mock<IMapper>();
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive
            });

        _mockEmailService = new Mock<IEmailService>();
        _mockEmailService
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _service = new AuthService(_context, _mockConfig.Object, _mockMapper.Object, _mockEmailService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================================
    // LoginAsync
    // ============================================================

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var dto = new LoginDto { Email = "notfound@example.com", Password = "any" };

        // Act
        var act = () => _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Tài khoản không tồn tại.");
    }

    [Fact]
    public async Task LoginAsync_GoogleAccount_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "google@example.com", passwordHash: "");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { Email = "google@example.com", Password = "any" };

        // Act
        var act = () => _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Google*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "test@example.com", passwordHash: BCrypt.Net.BCrypt.HashPassword("correct"));
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { Email = "test@example.com", Password = "wrong" };

        // Act
        var act = () => _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Mật khẩu không chính xác.");
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "inactive@example.com", isActive: false);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { Email = "inactive@example.com", Password = "password" };

        // Act
        var act = () => _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Tài khoản đã bị khóa.");
    }

    [Fact]
    public async Task LoginAsync_UnverifiedEmail_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "unverified@example.com", isEmailVerified: false);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { Email = "unverified@example.com", Password = "password" };

        // Act
        var act = () => _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*chưa xác thực email*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = CreateUser(email: "valid@example.com", isEmailVerified: true);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { Email = "valid@example.com", Password = "password" };

        // Act
        var result = await _service.LoginAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("valid@example.com");
    }

    // ============================================================
    // RegisterAsync
    // ============================================================

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "existing@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto { Email = "existing@example.com", Password = "password", FullName = "Test" };

        // Act
        var act = () => _service.RegisterAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email đã tồn tại.");
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_CreatesUserAndSendsEmail()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "password123",
            FullName = "New User",
            Phone = "0123456789"
        };

        // Act
        var result = await _service.RegisterAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.IsEmailVerified.Should().BeFalse();
        savedUser.EmailVerificationOtp.Should().NotBeNullOrEmpty();

        _mockEmailService.Verify(e =>
            e.SendEmailAsync("newuser@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ============================================================
    // ResetPasswordAsync
    // ============================================================

    [Fact]
    public async Task ResetPasswordAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var dto = new ResetPasswordDto { Email = "nope@example.com", OtpCode = "123456", NewPassword = "newpass" };

        // Act
        var act = () => _service.ResetPasswordAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email không tồn tại");
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidOtp_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "reset@example.com");
        _context.Users.Add(user);

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            OtpCode = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        var dto = new ResetPasswordDto { Email = "reset@example.com", OtpCode = "999999", NewPassword = "newpass" };

        // Act
        var act = () => _service.ResetPasswordAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*OTP không hợp lệ*");
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidOtp_ResetsPassword()
    {
        // Arrange
        var user = CreateUser(email: "resetok@example.com");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass");
        _context.Users.Add(user);

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            OtpCode = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        var dto = new ResetPasswordDto { Email = "resetok@example.com", OtpCode = "123456", NewPassword = "newpass123" };

        // Act
        await _service.ResetPasswordAsync(dto, CancellationToken.None);

        // Assert
        var updatedUser = await _context.Users.FirstAsync(u => u.Email == "resetok@example.com");
        BCrypt.Net.BCrypt.Verify("newpass123", updatedUser.PasswordHash).Should().BeTrue();

        var usedToken = await _context.PasswordResetTokens.FirstAsync(t => t.Id == token.Id);
        usedToken.IsUsed.Should().BeTrue();
    }

    // ============================================================
    // RefreshTokenAsync
    // ============================================================

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsException()
    {
        // Act
        var act = () => _service.RefreshTokenAsync("invalid-token", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Refresh token không hợp lệ.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ThrowsException()
    {
        // Arrange
        var user = CreateUser(email: "refresh@example.com");
        _context.Users.Add(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var act = () => _service.RefreshTokenAsync("expired-token", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*hết hạn*");
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var user = CreateUser(email: "refreshok@example.com");
        _context.Users.Add(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RefreshTokenAsync("valid-refresh-token", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe("valid-refresh-token"); // Should be rotated

        // Old token should be revoked
        var oldToken = await _context.RefreshTokens.FirstAsync(t => t.Token == "valid-refresh-token");
        oldToken.IsRevoked.Should().BeTrue();
    }

    // ============================================================
    // VerifyEmailAsync
    // ============================================================

    [Fact]
    public async Task VerifyEmailAsync_UserNotFound_ThrowsException()
    {
        var dto = new VerifyEmailDto { Email = "nope@example.com", OtpCode = "123456" };
        var act = () => _service.VerifyEmailAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Email không tồn tại.");
    }

    [Fact]
    public async Task VerifyEmailAsync_AlreadyVerified_ThrowsException()
    {
        var user = CreateUser(email: "verified@example.com", isEmailVerified: true);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new VerifyEmailDto { Email = "verified@example.com", OtpCode = "123456" };
        var act = () => _service.VerifyEmailAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*đã được xác nhận*");
    }

    [Fact]
    public async Task VerifyEmailAsync_WrongOtp_ThrowsException()
    {
        var user = CreateUser(email: "wrongotp@example.com", isEmailVerified: false);
        user.EmailVerificationOtp = "111111";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new VerifyEmailDto { Email = "wrongotp@example.com", OtpCode = "999999" };
        var act = () => _service.VerifyEmailAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*OTP không chính xác*");
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredOtp_ThrowsException()
    {
        var user = CreateUser(email: "expiredotp@example.com", isEmailVerified: false);
        user.EmailVerificationOtp = "123456";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(-1); // Expired
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new VerifyEmailDto { Email = "expiredotp@example.com", OtpCode = "123456" };
        var act = () => _service.VerifyEmailAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*hết hạn*");
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidOtp_VerifiesEmail()
    {
        var user = CreateUser(email: "verifyok@example.com", isEmailVerified: false);
        user.EmailVerificationOtp = "123456";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new VerifyEmailDto { Email = "verifyok@example.com", OtpCode = "123456" };

        await _service.VerifyEmailAsync(dto, CancellationToken.None);

        var updated = await _context.Users.FirstAsync(u => u.Email == "verifyok@example.com");
        updated.IsEmailVerified.Should().BeTrue();
        updated.EmailVerificationOtp.Should().BeNull();
        updated.OtpExpiresAt.Should().BeNull();
    }

    // ============================================================
    // LogoutAsync
    // ============================================================

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        var user = CreateUser(email: "logout@example.com");
        _context.Users.Add(user);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Token = "logout-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        await _service.LogoutAsync("logout-token", CancellationToken.None);

        var revokedToken = await _context.RefreshTokens.FirstAsync(t => t.Token == "logout-token");
        revokedToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_InvalidToken_DoesNotThrow()
    {
        // Act - should not throw
        await _service.LogoutAsync("nonexistent-token", CancellationToken.None);
    }

    // ============================================================
    // ForgotPasswordAsync
    // ============================================================

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ThrowsException()
    {
        var dto = new ForgotPasswordDto { Email = "nope@example.com" };
        var act = () => _service.ForgotPasswordAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Email không tồn tại trong hệ thống");
    }

    [Fact]
    public async Task ForgotPasswordAsync_GoogleAccount_ThrowsException()
    {
        var user = CreateUser(email: "google2@example.com", passwordHash: "");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ForgotPasswordDto { Email = "google2@example.com" };
        var act = () => _service.ForgotPasswordAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*Google*");
    }

    [Fact]
    public async Task ForgotPasswordAsync_ValidUser_SendsOtpEmail()
    {
        var user = CreateUser(email: "forgot@example.com");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ForgotPasswordDto { Email = "forgot@example.com" };

        await _service.ForgotPasswordAsync(dto, CancellationToken.None);

        var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId);
        token.Should().NotBeNull();
        token!.IsUsed.Should().BeFalse();

        _mockEmailService.Verify(e =>
            e.SendEmailAsync("forgot@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ============================================================
    // ChangePasswordAsync
    // ============================================================

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ThrowsException()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "newpass" };
        var act = () => _service.ChangePasswordAsync(Guid.NewGuid(), dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Không tìm thấy người dùng.");
    }

    [Fact]
    public async Task ChangePasswordAsync_GoogleAccount_ThrowsException()
    {
        var user = CreateUser(email: "gchange@example.com", passwordHash: "");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "newpass" };
        var act = () => _service.ChangePasswordAsync(user.UserId, dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*Google*");
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsException()
    {
        var user = CreateUser(email: "wrongcur@example.com");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ChangePasswordDto { CurrentPassword = "wrong", NewPassword = "newpass" };
        var act = () => _service.ChangePasswordAsync(user.UserId, dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Mật khẩu hiện tại không chính xác.");
    }

    [Fact]
    public async Task ChangePasswordAsync_SamePassword_ThrowsException()
    {
        var user = CreateUser(email: "same@example.com");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("samepassword");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ChangePasswordDto { CurrentPassword = "samepassword", NewPassword = "samepassword" };
        var act = () => _service.ChangePasswordAsync(user.UserId, dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*trùng với mật khẩu cũ*");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidInput_ChangesPassword()
    {
        var user = CreateUser(email: "changeok@example.com");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ChangePasswordDto { CurrentPassword = "oldpassword", NewPassword = "newpassword" };

        await _service.ChangePasswordAsync(user.UserId, dto, CancellationToken.None);

        var updated = await _context.Users.FirstAsync(u => u.UserId == user.UserId);
        BCrypt.Net.BCrypt.Verify("newpassword", updated.PasswordHash).Should().BeTrue();
    }

    // ============================================================
    // ResendEmailAsync
    // ============================================================

    [Fact]
    public async Task ResendEmailAsync_UserNotFound_ThrowsException()
    {
        var dto = new ResendEmailDto { Email = "nope@example.com", Type = "verify" };
        var act = () => _service.ResendEmailAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Email không tồn tại.");
    }

    [Fact]
    public async Task ResendEmailAsync_VerifyType_SendsNewOtp()
    {
        var user = CreateUser(email: "resend@example.com", isEmailVerified: false);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new ResendEmailDto { Email = "resend@example.com", Type = "verify" };
        await _service.ResendEmailAsync(dto, CancellationToken.None);

        var updated = await _context.Users.FirstAsync(u => u.Email == "resend@example.com");
        updated.EmailVerificationOtp.Should().NotBeNullOrEmpty();
        updated.OtpExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _mockEmailService.Verify(e =>
            e.SendEmailAsync("resend@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ============================================================
    // Helper
    // ============================================================

    private static User CreateUser(
        string email = "test@example.com",
        string passwordHash = "hashed",
        bool isActive = true,
        bool isEmailVerified = true)
    {
        return new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FullName = "Test User",
            Role = UserRole.customer,
            IsActive = isActive,
            IsEmailVerified = isEmailVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
