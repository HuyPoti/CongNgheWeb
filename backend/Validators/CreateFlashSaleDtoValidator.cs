using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class CreateFlashSaleDtoValidator : AbstractValidator<CreateFlashSaleDto>
{
    public CreateFlashSaleDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.StartTime)
            .NotEmpty();

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .GreaterThan(x => x.StartTime);
    }
}
