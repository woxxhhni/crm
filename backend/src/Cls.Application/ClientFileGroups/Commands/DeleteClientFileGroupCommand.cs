using MediatR;
using Cls.Application.Abstractions;

namespace Cls.Application.ClientFileGroups.Commands;

public record DeleteClientFileGroupCommand(int Id) : IRequest<bool>;

public class DeleteClientFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteClientFileGroupCommand, bool>
{
    public async Task<bool> Handle(DeleteClientFileGroupCommand r, CancellationToken ct)
    {
        var e = await uow.ClientFileGroups.GetByIdAsync(r.Id, ct);
        if (e is null) return false;
        await uow.ClientFileGroups.DeleteAsync(e, ct);
        return true;
    }
}
