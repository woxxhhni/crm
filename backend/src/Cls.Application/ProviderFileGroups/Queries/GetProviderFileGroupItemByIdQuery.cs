using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ProviderFileGroups.Queries;

public record GetProviderFileGroupItemByIdQuery(int Id) : IRequest<ProviderFileGroupItem?>;

public class GetProviderFileGroupItemByIdHandler(IUnitOfWork uow) : IRequestHandler<GetProviderFileGroupItemByIdQuery, ProviderFileGroupItem?>
{
    public async Task<ProviderFileGroupItem?> Handle(GetProviderFileGroupItemByIdQuery r, CancellationToken ct)
    {
        return await uow.ProviderFileGroupItems.Query().FirstOrDefaultAsync(g => g.Id == r.Id, ct);
    }
}
