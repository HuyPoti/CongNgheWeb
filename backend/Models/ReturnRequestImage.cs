using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("return_request_images")]
public class ReturnRequestImage
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("return_id")]
    public Guid ReturnId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ReturnId")]
    public ReturnRequest? ReturnRequest { get; set; }
}
