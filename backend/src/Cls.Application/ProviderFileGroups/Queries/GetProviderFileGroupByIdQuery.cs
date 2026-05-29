using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ProviderFileGroups.Queries;

public record GetProviderFileGroupByIdQuery(int Id) : IRequest<ProviderFileGroup?>;

public class GetProviderFileGroupByIdHandler(IUnitOfWork uow) : IRequestHandler<GetProviderFileGroupByIdQuery, ProviderFileGroup?>
{
    public async Task<ProviderFileGroup?> Handle(GetProviderFileGroupByIdQuery r, CancellationToken ct)
    {
        return await uow.ProviderFileGroups.Query()
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == r.Id, ct);
    }
}
