using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
namespace Cls.Application.Providers.Commands;
public record DeleteProviderCommand(int Id) : IRequest<bool>;
public class DeleteProviderCommandHandler(IUnitOfWork uow, IDeletionGuard<Provider> deletionGuard) : IRequestHandler<DeleteProviderCommand, bool>
{
    public async Task<bool> Handle(DeleteProviderCommand r, CancellationToken ct)
    {
        var e = await uow.Providers.GetByIdAsync(r.Id, ct);
        if (e is null) 
            return false;
        await deletionGuard.EnsureCanDeleteAsync(e, ct);
        await uow.Providers.DeleteAsync(e, ct);
        return true;
    }
}
