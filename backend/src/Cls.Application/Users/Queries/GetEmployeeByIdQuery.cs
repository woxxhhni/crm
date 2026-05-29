using Cls.Application.Abstractions;
using Cls.Application.Users.Commands;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Users.Queries;

public record GetEmployeeByIdQuery(int Id) : IRequest<User?>;

public class GetEmployeeByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetEmployeeByIdQuery, User?>
{
    public async Task<User?> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var user = await uow.Users.Query()
            .Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (user?.Role == UserRole.Admin)
            return null;

        return user;
    }
}
