using MediatR; 
using Cls.Application.Abstractions; 
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Users.Queries; 
public record GetUserByIdQuery(int Id) : IRequest<User?>;

public class GetUserByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetUserByIdQuery, User?>
{
    public Task<User?> Handle(GetUserByIdQuery request, CancellationToken ct) => uow.Users
        .Query()
        .Include(x=>x.File)
        .FirstOrDefaultAsync(x=> x.Id == request.Id, ct);
}
