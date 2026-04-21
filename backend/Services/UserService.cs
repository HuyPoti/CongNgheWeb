// Implement IUserService
// Nơi xử lý logic: chuyển đổi DTO ↔ Model, validate, hash password

using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.DTOs;
using backend.Models;
using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class UserService(IUnitOfWork uow, IMapper mapper) : IUserService
{
    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct)
    {
        var users = await uow.Users.Query()
            .Where(u => u.IsActive)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        return users;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct) => await uow.Users.GetByIdAsync<UserDto>(id, ct);
    public async Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken ct)
    {
        var existing = await uow.Users.GetAsync<UserDto>(u => u.Email == dto.Email, ct);
        if (existing.Any()) return null;

        var entity = mapper.Map<User>(dto);
        entity.PasswordHash = HashPassword(dto.Password);
        
        uow.Users.Insert(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<UserDto>(entity);
    }
    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct)
    {
        var entity = await uow.Users.Query().FirstOrDefaultAsync(u => u.UserId == id, ct);
        if (entity == null) return null;

        if (dto.Email != null)
        {
            var existing = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
            if (existing != null && existing.UserId != id) return null;
        }

        mapper.Map(dto, entity);
        uow.Users.Update(entity);
        await uow.SaveAsync(ct);
        return mapper.Map<UserDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.Users.Query().FirstOrDefaultAsync(u => u.UserId == id, ct);
        if (entity == null) return false;

        entity.IsActive = false;
        uow.Users.Update(entity);
        await uow.SaveAsync(ct);
        return true;
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}