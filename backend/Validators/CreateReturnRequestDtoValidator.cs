using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class CreateReturnRequestDtoValidator : AbstractValidator<CreateReturnRequestDto>
{
    public CreateReturnRequestDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be selected for return");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.OrderItemId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
