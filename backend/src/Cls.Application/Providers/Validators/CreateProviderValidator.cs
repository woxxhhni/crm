using FluentValidation;
using Cls.Application.Providers.Commands;
namespace Cls.Application.Providers.Validators;
public class CreateProviderValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
        RuleFor(x => x.SecondPhone).MaximumLength(50).When(x => x.SecondPhone != null);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(255);
        RuleFor(x => x.Website).MaximumLength(255).When(x => x.Website != null);
    }
}
