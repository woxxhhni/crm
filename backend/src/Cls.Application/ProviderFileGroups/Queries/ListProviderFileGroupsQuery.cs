using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ProviderFileGroups.Queries;

public record ListProviderFileGroupsQuery(int ProviderId) : IRequest<List<ProviderFileGroup>>;

public class ListProviderFileGroupsHandler(IUnitOfWork uow) : IRequestHandler<ListProviderFileGroupsQuery, List<ProviderFileGroup>>
{
    public async Task<List<ProviderFileGroup>> Handle(ListProviderFileGroupsQuery r, CancellationToken ct)
    {
        return await uow.ProviderFileGroups.Query()
            .Include(g => g.Items)
            .ThenInclude(g => g.File)
            .Where(g => g.ProviderId == r.ProviderId)
            .OrderBy(g => g.Label)
            .ToListAsync(ct);
    }
}
