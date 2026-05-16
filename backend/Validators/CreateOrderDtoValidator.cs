using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.ProductId).NotEmpty();
        });
        RuleFor(x => x.PaymentMethod).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotNull().When(x => x.ShippingAddressId == null);
        RuleFor(x => x.ShippingAddressId).NotNull().When(x => x.ShippingAddress == null);

        RuleFor(x => x.ShippingAddress!.RecipientName).NotEmpty().When(x => x.ShippingAddress != null);
        RuleFor(x => x.ShippingAddress!.Phone).NotEmpty().When(x => x.ShippingAddress != null);
        RuleFor(x => x.ShippingAddress!.AddressLine).NotEmpty().When(x => x.ShippingAddress != null);
    }
}
