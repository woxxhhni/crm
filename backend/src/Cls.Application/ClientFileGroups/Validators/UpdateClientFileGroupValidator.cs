using FluentValidation;
using Cls.Application.ClientFileGroups.Commands;

namespace Cls.Application.ClientFileGroups.Validators;

public class UpdateClientFileGroupValidator : AbstractValidator<UpdateClientFileGroupCommand>
{
    public UpdateClientFileGroupValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(255);
    }
}
