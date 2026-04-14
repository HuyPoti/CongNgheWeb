namespace backend.DTOs;

public class ReviewImageDto
{
    public Guid ImageId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
