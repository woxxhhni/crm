using FluentValidation;
using Cls.Application.ProviderFileGroups.Commands;

namespace Cls.Application.ProviderFileGroups.Validators;

public class CreateProviderFileGroupValidator : AbstractValidator<CreateProviderFileGroupCommand>
{
    public CreateProviderFileGroupValidator()
    {
        RuleFor(x => x.ProviderId).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CreatedByUserId).GreaterThan(0);
    }
}
