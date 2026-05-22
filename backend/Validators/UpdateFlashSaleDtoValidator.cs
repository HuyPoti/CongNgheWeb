using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class UpdateFlashSaleDtoValidator : AbstractValidator<UpdateFlashSaleDto>
{
    public UpdateFlashSaleDtoValidator()
    {
        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(100)
            .When(x => x.Title != null);

        RuleFor(x => x)
            .Must(x => !x.StartTime.HasValue || !x.EndTime.HasValue || x.EndTime.Value > x.StartTime.Value)
            .WithMessage("EndTime must be greater than StartTime");
    }
}
