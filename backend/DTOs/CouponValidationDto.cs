using System.Collections.Generic;

namespace backend.DTOs;

public class CouponValidationItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CouponValidationRequestDto
{
    public string Code { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid? UserId { get; set; }
    public List<CouponValidationItemDto>? Items { get; set; }
}

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public Guid? CouponId { get; set; }
    public string? Code { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
