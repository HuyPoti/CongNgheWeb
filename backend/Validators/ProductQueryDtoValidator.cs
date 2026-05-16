using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class ProductQueryDtoValidator : AbstractValidator<ProductQueryDto>
{
    public ProductQueryDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice!.Value)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice");
            
        RuleFor(x => x.Keyword).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Keyword));
    }
}
