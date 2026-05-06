using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateReturnRequestDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Reason { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public List<CreateReturnRequestItemDto> Items { get; set; } = new();

    public List<string> ImageUrls { get; set; } = new();
}

public class CreateReturnRequestItemDto
{
    [Required]
    public Guid OrderItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string? ReasonDetail { get; set; }
}
