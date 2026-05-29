using FluentValidation;

namespace Cls.Application.Auth.Validators;

public class AuthenticateCommandValidator : AbstractValidator<Commands.AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
