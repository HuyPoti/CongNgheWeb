using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(255);
        RuleFor(x => x.RegularPrice).GreaterThan(0);
        RuleFor(x => x.SalePrice).GreaterThan(0).LessThan(x => x.RegularPrice).When(x => x.SalePrice.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandId).NotEmpty();
    }
}
