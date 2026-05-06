using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SupplierDto
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupplierDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    public string? Address { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateSupplierDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
