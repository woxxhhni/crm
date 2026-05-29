using MediatR;
using Cls.Application.Abstractions;

namespace Cls.Application.ProviderFileGroups.Commands;

public record DeleteProviderFileGroupCommand(int Id) : IRequest<bool>;

public class DeleteProviderFileGroupCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteProviderFileGroupCommand, bool>
{
    public async Task<bool> Handle(DeleteProviderFileGroupCommand r, CancellationToken ct)
    {
        var e = await uow.ProviderFileGroups.GetByIdAsync(r.Id, ct);
        if (e is null) return false;
        await uow.ProviderFileGroups.DeleteAsync(e, ct);
        return true;
    }
}
