using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.PerUserLimit).GreaterThan(0);
    }
}
