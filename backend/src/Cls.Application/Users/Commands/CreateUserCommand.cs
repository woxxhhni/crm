using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Users.Helpers;
using Cls.Application.Users.Services;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Mapping;
using MediatR;

namespace Cls.Application.Users.Commands;

public record CreateUserCommand(
    string Name,
    string Email,
    string PasswordPlain,
    string? Phone,
    string? Address,
    string? Description,
    UserRole Role,
    bool IsActive,
    int? FileId = null) : IRequest<User>, IOneWayMap<User>;

public class CreateUserCommandHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, User>
{
    public Task<User> Handle(CreateUserCommand request, CancellationToken ct) =>
        UserCommandHelpers.CreateAsync(
            uow,
            mapper,
            passwordHasher,
            request.Name,
            request.Email,
            request.PasswordPlain,
            request.Phone,
            request.Address,
            request.Description,
            request.Role,
            request.IsActive,
            request.FileId,
            ct);
}
