using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("product_specs")]
public class ProductSpec
{
    [Key]
    [Column("spec_id")]
    public Guid SpecId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("spec_key")]
    public string SpecKey { get; set; } = string.Empty;

    [Column("spec_value")]
    public string SpecValue { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
}