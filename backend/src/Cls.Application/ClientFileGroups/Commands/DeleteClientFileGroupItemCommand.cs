using MediatR;
using Cls.Application.Abstractions;

namespace Cls.Application.ClientFileGroups.Commands;

public record DeleteClientFileGroupItemCommand(int ItemId) : IRequest<bool>;

public class DeleteClientFileGroupItemCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteClientFileGroupItemCommand, bool>
{
    public async Task<bool> Handle(DeleteClientFileGroupItemCommand r, CancellationToken ct)
    {
        var e = await uow.ClientFileGroupItems.GetByIdAsync(r.ItemId, ct);
        if (e is null) return false;
        await uow.ClientFileGroupItems.DeleteAsync(e, ct);
        return true;
    }
}
