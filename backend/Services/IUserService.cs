// Interface cho Service layer
// Service xử lý LOGIC NGHIỆP VỤ (business logic)
// Ví dụ: kiểm tra email trùng, hash password, validate dữ liệu

using backend.DTOs;

namespace backend.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken ct);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken ct);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
