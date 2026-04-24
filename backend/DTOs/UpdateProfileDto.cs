using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateProfileDto
{
    [MaxLength(100)]
    public string? FullName {get; set; }

    [MaxLength(20)]
    public string? Phone {get; set;}

    [MaxLength(500)]
    public string? AvatarUrl {get; set;}
}