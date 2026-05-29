using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ClientFileGroups.Queries;

public record GetClientFileGroupByIdQuery(int Id) : IRequest<ClientFileGroup?>;

public class GetClientFileGroupByIdHandler(IUnitOfWork uow) : IRequestHandler<GetClientFileGroupByIdQuery, ClientFileGroup?>
{
    public async Task<ClientFileGroup?> Handle(GetClientFileGroupByIdQuery r, CancellationToken ct)
    {
        return await uow.ClientFileGroups.Query()
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == r.Id, ct);
    }
}
