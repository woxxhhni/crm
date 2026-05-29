using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ClientFileGroups.Queries;

public record ListClientFileGroupsQuery(int ClientId) : IRequest<List<ClientFileGroup>>;

public class ListClientFileGroupsHandler(IUnitOfWork uow) : IRequestHandler<ListClientFileGroupsQuery, List<ClientFileGroup>>
{
    public async Task<List<ClientFileGroup>> Handle(ListClientFileGroupsQuery r, CancellationToken ct)
    {
        return await uow.ClientFileGroups.Query()
            .Include(g => g.Items)
            .ThenInclude(g => g.File)
            .Where(g => g.ClientId == r.ClientId)
            .OrderBy(g => g.Label)
            .ToListAsync(ct);
    }
}
