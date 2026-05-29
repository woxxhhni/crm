using FluentValidation;
using Cls.Application.Clients.Commands;
namespace Cls.Application.Clients.Validators;
public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
        RuleFor(x => x.SecondPhone).MaximumLength(50).When(x => x.SecondPhone != null);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(255);
        RuleFor(x => x.Website).MaximumLength(255).When(x => x.Website != null);
    }
}
