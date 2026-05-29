using Cls.Application.Orders.Commands;
using FluentValidation;

namespace Cls.Application.Orders.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.BuyCurrency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.SellCurrency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.BuyAmount).GreaterThan(0);
        RuleFor(x => x.SellAmount).GreaterThan(0);
        RuleFor(x => x.ClientId).NotNull().GreaterThan(0);
        RuleFor(x => x.ProviderId).NotNull().GreaterThan(0);
    }
}
