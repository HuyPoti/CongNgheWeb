using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;




[Table("product_images")]
public class ProductImage
{
    [Key]
    [Column("image_id")]
    public Guid ProductImageId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
}