using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.ClientFileGroups.Commands;

public record AddClientFileGroupItemsCommand(int GroupId, List<int> FileIds) : IRequest<int>;

public class AddClientFileGroupItemsCommandHandler(IUnitOfWork uow) : IRequestHandler<AddClientFileGroupItemsCommand, int>
{
    public async Task<int> Handle(AddClientFileGroupItemsCommand r, CancellationToken ct)
    {
        var group = await uow.ClientFileGroups.GetByIdAsync(r.GroupId, ct);
        if (group is null) return 0;
        int count = 0;
        foreach (var fid in r.FileIds.Distinct())
        {
            await uow.ClientFileGroupItems.AddAsync(new ClientFileGroupItem { ClientFileGroupId = r.GroupId, FileId = fid }, ct);
            count++;
        }
        return count;
    }
}
