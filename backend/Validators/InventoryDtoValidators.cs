using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class CreateInventoryReceiptDtoValidator : AbstractValidator<CreateInventoryReceiptDto>
{
    public CreateInventoryReceiptDtoValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Receipt must have at least one item");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class AdjustStockDtoValidator : AbstractValidator<AdjustStockDto>
{
    public AdjustStockDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.QuantityChanged).NotEqual(0).WithMessage("Quantity changed must not be zero");
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(500);
    }
}
