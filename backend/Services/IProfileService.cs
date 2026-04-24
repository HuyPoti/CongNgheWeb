using backend.DTOs;

namespace backend.Services;

public interface IProfileService
{
    Task<UserDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken);
}