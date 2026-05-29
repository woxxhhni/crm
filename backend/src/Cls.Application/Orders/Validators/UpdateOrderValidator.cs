using Cls.Application.Orders.Commands;
using FluentValidation;

namespace Cls.Application.Orders.Validators;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.BuyCurrency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.SellCurrency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.BuyAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellAmount).GreaterThanOrEqualTo(0);
    }
}
