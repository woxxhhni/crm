using MediatR;
using Cls.Application.Abstractions;

namespace Cls.Application.ProviderFileGroups.Commands;

public record DeleteProviderFileGroupItemCommand(int ItemId) : IRequest<bool>;

public class DeleteProviderFileGroupItemCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteProviderFileGroupItemCommand, bool>
{
    public async Task<bool> Handle(DeleteProviderFileGroupItemCommand r, CancellationToken ct)
    {
        var e = await uow.ProviderFileGroupItems.GetByIdAsync(r.ItemId, ct);
        if (e is null) return false;
        await uow.ProviderFileGroupItems.DeleteAsync(e, ct);
        return true;
    }
}
