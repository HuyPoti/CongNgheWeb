using backend.DTOs;
using FluentValidation;

namespace backend.Validators;

public class CreateFlashSaleDtoValidator : AbstractValidator<CreateFlashSaleDto>
{
    public CreateFlashSaleDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
    }
}
