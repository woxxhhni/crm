using FluentValidation;

namespace Cls.Application.Users.Validators;

public class UpdateEmployeeCommandValidator : AbstractValidator<Commands.UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
    }
}
