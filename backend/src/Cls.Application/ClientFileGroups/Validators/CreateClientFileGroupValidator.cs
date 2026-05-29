using FluentValidation;
using Cls.Application.ClientFileGroups.Commands;

namespace Cls.Application.ClientFileGroups.Validators;

public class CreateClientFileGroupValidator : AbstractValidator<CreateClientFileGroupCommand>
{
    public CreateClientFileGroupValidator()
    {
        RuleFor(x => x.ClientId).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CreatedByUserId).GreaterThan(0);
    }
}
