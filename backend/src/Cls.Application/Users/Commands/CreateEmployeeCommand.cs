using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Users.Helpers;
using Cls.Application.Users.Services;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using MediatR;

namespace Cls.Application.Users.Commands;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    string PasswordPlain,
    string? Phone,
    string? Address,
    string? Description,
    string Role,
    bool IsActive,
    int? FileId = null) : IRequest<User>;

public class CreateEmployeeCommandHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateEmployeeCommand, User>
{
    public Task<User> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        var role = UserRoleGuards.ParseRole(request.Role);
        return UserCommandHelpers.CreateAsync(
            uow,
            mapper,
            passwordHasher,
            request.Name,
            request.Email,
            request.PasswordPlain,
            request.Phone,
            request.Address,
            request.Description,
            role,
            request.IsActive,
            request.FileId,
            ct);
    }
}
