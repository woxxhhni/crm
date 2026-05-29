using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ProviderFileGroups.Commands;

public record UpdateProviderFileGroupCommand(int Id, string Label) : IRequest<ProviderFileGroup?>;

public class UpdateProviderFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateProviderFileGroupCommand, ProviderFileGroup?>
{
    public async Task<ProviderFileGroup?> Handle(UpdateProviderFileGroupCommand r, CancellationToken ct)
    {
        var e = await uow.ProviderFileGroups.GetByIdAsync(r.Id, ct);
        if (e is null) return null;
        var existItem = await uow.ProviderFileGroups
            .Query().FirstOrDefaultAsync(x => x.ProviderId == e.ProviderId && x.Label == r.Label && !x.IsDeleted && x.Id != e.Id, ct);
        if (existItem is not null)
            throw new Shared.Exceptions.InvalidActionException($"A ProviderFileGroup with label '{r.Label}' already exists for ProviderId {e.ProviderId}.");

        e.Label = r.Label;
        e.UpdatedAt = DateTime.UtcNow;
        await uow.ProviderFileGroups.UpdateAsync(e, ct);
        return e;
    }
}
