using FluentValidation;

namespace Cls.Application.Users.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<Commands.ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}
