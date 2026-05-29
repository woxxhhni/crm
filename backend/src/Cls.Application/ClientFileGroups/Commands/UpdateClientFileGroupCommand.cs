using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ClientFileGroups.Commands;

public record UpdateClientFileGroupCommand(int Id, string Label) : IRequest<ClientFileGroup?>;

public class UpdateClientFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateClientFileGroupCommand, ClientFileGroup?>
{
    public async Task<ClientFileGroup?> Handle(UpdateClientFileGroupCommand r, CancellationToken ct)
    {
        var e = await uow.ClientFileGroups.GetByIdAsync(r.Id, ct);
        if (e is null) return null;
        var existItem = await uow.ClientFileGroups
            .Query().FirstOrDefaultAsync(x => x.ClientId == e.ClientId && x.Label == r.Label && !x.IsDeleted && x.Id != e.Id, ct);
        if (existItem is not null)
            throw new Shared.Exceptions.InvalidActionException($"A ClientFileGroup with label '{r.Label}' already exists for ClientId {e.ClientId}.");

        e.Label = r.Label;
        e.UpdatedAt = DateTime.UtcNow;
        await uow.ClientFileGroups.UpdateAsync(e, ct);
        return e;
    }
}
