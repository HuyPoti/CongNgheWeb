using backend.Models;

namespace backend.DTOs;

public class UpdateUserDto
{
    public string? Email {get; set; }
    public string? FullName {get; set; }
    public string? Phone {get; set; }
    public UserRole? Role {get; set; }
}