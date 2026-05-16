using AutoMapper;
using backend.UnitOfWork;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProfileService(IUnitOfWork uow, IMapper mapper) : IProfileService
{
    public async Task<UserDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        return user != null ? mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct)
    {
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.UserId == userId, ct);
        if (user == null) return null;

        // Chỉ cập nhật field được gửi lên (không null)
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await uow.SaveAsync(ct);

        return mapper.Map<UserDto>(user);
    }
}