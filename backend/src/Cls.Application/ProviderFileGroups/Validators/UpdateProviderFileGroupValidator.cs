using FluentValidation;
using Cls.Application.ProviderFileGroups.Commands;

namespace Cls.Application.ProviderFileGroups.Validators;

public class UpdateProviderFileGroupValidator : AbstractValidator<UpdateProviderFileGroupCommand>
{
    public UpdateProviderFileGroupValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(255);
    }
}
