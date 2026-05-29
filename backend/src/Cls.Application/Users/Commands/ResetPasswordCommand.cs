using Cls.Application.Abstractions;
using Cls.Application.Users.Services;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using MediatR;

namespace Cls.Application.Users.Commands;

public record ResetPasswordCommand(int Id, string NewPassword, string ConfirmPassword) : IRequest<User>;

public class ResetPasswordCommandHandler(IUnitOfWork uow, IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, User>
{
    public async Task<User> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (!request.NewPassword.Equals(request.ConfirmPassword))
            throw new BadRequestException("New password and confirm password do not match");

        var user = await uow.Users.GetByIdAsync(request.Id, ct);
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await uow.Users.UpdateAsync(user, ct);
        return user;
    }
}
