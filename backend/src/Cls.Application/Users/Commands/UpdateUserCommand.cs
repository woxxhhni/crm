using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Users.Helpers;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Mapping;
using MediatR;

namespace Cls.Application.Users.Commands;

public record UpdateUserCommand(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string? Address,
    string? Description,
    UserRole Role,
    bool IsActive,
    int? FileId = null) : IRequest<User?>, IOneWayMap<User>;

public class UpdateUserCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<UpdateUserCommand, User?>
{
    public Task<User?> Handle(UpdateUserCommand request, CancellationToken ct) =>
        UserCommandHelpers.UpdateAsync(
            uow,
            mapper,
            request.Id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.Description,
            request.Role,
            request.IsActive,
            request.FileId,
            ct);
}
