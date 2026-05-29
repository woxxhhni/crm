using FluentValidation;

namespace Cls.Application.Users.Validators;

public class CreateEmployeeCommandValidator : AbstractValidator<Commands.CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.PasswordPlain).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
    }
}
