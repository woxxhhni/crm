using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.Clients.Commands;
public record DeleteClientCommand(int Id) : IRequest<bool>;
public class DeleteClientCommandHandler(IUnitOfWork uow, IDeletionGuard<Client> deletionGuard) : IRequestHandler<DeleteClientCommand, bool>
{
    public async Task<bool> Handle(DeleteClientCommand r, CancellationToken ct)
    {
        var e = await uow.Clients.GetByIdAsync(r.Id, ct);
        if (e is null)
            return false;
        await deletionGuard.EnsureCanDeleteAsync(e, ct);
        await uow.Clients.DeleteAsync(e, ct);
        return true;
    }
}
