using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ProviderFileGroups.Commands;

public record CreateProviderFileGroupCommand(int ProviderId, string Label, int CreatedByUserId, List<int>? FileIds) : IRequest<ProviderFileGroup>;

public class CreateProviderFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateProviderFileGroupCommand, ProviderFileGroup>
{
    public async Task<ProviderFileGroup> Handle(CreateProviderFileGroupCommand r, CancellationToken ct)
    {
        var existItem = await uow.ProviderFileGroups
            .Query().FirstOrDefaultAsync(x => x.ProviderId == r.ProviderId && x.Label == r.Label && !x.IsDeleted, ct);
        if (existItem is not null)
            throw new Shared.Exceptions.InvalidActionException($"A ProviderFileGroup with label '{r.Label}' already exists for ProviderId {r.ProviderId}.");

        var entity = new ProviderFileGroup
        {
            ProviderId = r.ProviderId,
            Label = r.Label,
            CreatedByUserId = r.CreatedByUserId
        };
        if (r.FileIds is { Count: > 0 })
        {
            foreach (var fid in r.FileIds)
                entity.Items.Add(new ProviderFileGroupItem()
                {
                    FileId = fid,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = r.CreatedByUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = r.CreatedByUserId
                });
        }
        entity = await uow.ProviderFileGroups.AddAsync(entity, ct);
        return entity;
    }
}
