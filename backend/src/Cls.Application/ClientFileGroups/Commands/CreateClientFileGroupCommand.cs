using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using Cls.Shared.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.ClientFileGroups.Commands;

public record CreateClientFileGroupCommand(int ClientId, string Label, int CreatedByUserId, List<int>? FileIds)
    : IRequest<ClientFileGroup>, IOneWayMap<ClientFileGroup>;

public class CreateClientFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateClientFileGroupCommand, ClientFileGroup>
{
    public async Task<ClientFileGroup> Handle(CreateClientFileGroupCommand r, CancellationToken ct)
    {
        var existItem = await uow.ClientFileGroups
            .Query().FirstOrDefaultAsync(x => x.ClientId == r.ClientId && x.Label == r.Label && !x.IsDeleted, ct);
        if (existItem is not null)
            throw new Shared.Exceptions.InvalidActionException($"A ClientFileGroup with label '{r.Label}' already exists for ClientId {r.ClientId}.");
        var entity = new ClientFileGroup
        {
            ClientId = r.ClientId,
            Label = r.Label,
            CreatedByUserId = r.CreatedByUserId
        };
        if (r.FileIds is { Count: > 0 })
        {
            foreach (var fid in r.FileIds)
                entity.Items.Add(new ClientFileGroupItem()
                {
                    FileId = fid,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = r.CreatedByUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = r.CreatedByUserId
                });
        }
        entity = await uow.ClientFileGroups.AddAsync(entity, ct);
        return entity;
    }
}
