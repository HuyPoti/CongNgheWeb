using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class CreateReviewReplyDtoValidator : AbstractValidator<CreateReviewReplyDto>
{
    public CreateReviewReplyDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
    }
}
