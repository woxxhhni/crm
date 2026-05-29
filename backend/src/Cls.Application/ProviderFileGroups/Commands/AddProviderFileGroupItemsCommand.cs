using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.ProviderFileGroups.Commands;

public record AddProviderFileGroupItemsCommand(int GroupId, List<int> FileIds) : IRequest<int>;

public class AddProviderFileGroupItemsCommandHandler(IUnitOfWork uow) : IRequestHandler<AddProviderFileGroupItemsCommand, int>
{
    public async Task<int> Handle(AddProviderFileGroupItemsCommand r, CancellationToken ct)
    {
        var group = await uow.ProviderFileGroups.GetByIdAsync(r.GroupId, ct);
        if (group is null) return 0;
        int count = 0;
        foreach (var fid in r.FileIds.Distinct())
        {
            await uow.ProviderFileGroupItems.AddAsync(new ProviderFileGroupItem { ProviderFileGroupId = r.GroupId, FileId = fid }, ct);
            count++;
        }
        return count;
    }
}
