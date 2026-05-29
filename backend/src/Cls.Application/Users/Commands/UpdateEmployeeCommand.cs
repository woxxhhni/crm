using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Users.Helpers;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using MediatR;

namespace Cls.Application.Users.Commands;

public record UpdateEmployeeCommand(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string? Address,
    string? Description,
    string Role,
    bool IsActive,
    int? FileId = null) : IRequest<User?>;

public class UpdateEmployeeCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<UpdateEmployeeCommand, User?>
{
    public async Task<User?> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        var role = UserRoleGuards.ParseRole(request.Role);
        var updated = await UserCommandHelpers.UpdateAsync(
            uow,
            mapper,
            request.Id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.Description,
            role,
            request.IsActive,
            request.FileId,
            ct);

        if (updated is not null)
            updated.Role = UserRole.Employee;

        return updated;
    }
}
