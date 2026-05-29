using Cls.Application.Abstractions;
using Cls.Application.Users.Services;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Auth.Commands;

public record AuthenticateCommand(string Email, string Password) : IRequest<LoginResponse>;

public class AuthenticateCommandHandler(
    IUnitOfWork uow,
    IJwtTokenService jwt,
    IPasswordHasher passwordHasher) : IRequestHandler<AuthenticateCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(AuthenticateCommand request, CancellationToken ct)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await uow.Users.Query().FirstOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new UnauthorizedException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedException("Invalid credentials");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        user.LastLoginAt = DateTime.UtcNow;
        var expiresIn = DateTime.UtcNow;
        var token = jwt.Generate(user, out expiresIn);

        return new LoginResponse
        {
            AccessToken = token,
            ExpireIn = (int)(expiresIn - DateTime.UtcNow).TotalSeconds,
            TokenType = "Bearer"
        };
    }
}
