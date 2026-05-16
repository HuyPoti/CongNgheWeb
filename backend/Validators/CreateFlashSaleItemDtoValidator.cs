using FluentValidation;
using backend.DTOs;

namespace backend.Validators;

public class CreateFlashSaleItemDtoValidator : AbstractValidator<CreateFlashSaleItemDto>
{
    public CreateFlashSaleItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FlashPrice).GreaterThan(0);
        RuleFor(x => x.StockLimit).GreaterThan(0);
    }
}
