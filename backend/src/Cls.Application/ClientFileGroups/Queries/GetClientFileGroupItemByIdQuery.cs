using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ClientFileGroups.Queries;

public record GetClientFileGroupItemByIdQuery(int Id) : IRequest<ClientFileGroupItem?>;

public class GetClientFileGroupItemByIdHandler(IUnitOfWork uow) : IRequestHandler<GetClientFileGroupItemByIdQuery, ClientFileGroupItem?>
{
    public async Task<ClientFileGroupItem?> Handle(GetClientFileGroupItemByIdQuery r, CancellationToken ct)
    {
        return await uow.ClientFileGroupItems.Query().FirstOrDefaultAsync(g => g.Id == r.Id, ct);
    }
}
