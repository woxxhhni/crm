using Cls.Application.Abstractions;
using MediatR;
using Cls.Shared.Exceptions;

namespace Cls.Application.Orders.Commands.ExtraProviders;

public record RemoveOrderExtraProviderCommand(int OrderId, int ExtraProviderId, int UserId) : IRequest;

public class RemoveOrderExtraProviderCommandHandler(IUnitOfWork uow) : IRequestHandler<RemoveOrderExtraProviderCommand>
{
    public async Task Handle(RemoveOrderExtraProviderCommand r, CancellationToken ct)
    {
        var ep = await uow.ExtraProviders.GetByIdAsync(r.ExtraProviderId, ct);
        if (ep == null) throw new NotFoundException("Extra Provider not found");

        if (ep.OrderId != r.OrderId) throw new InvalidActionException("Extra Provider does not belong to this order");

        ep.Remove(r.UserId);
        await uow.ExtraProviders.UpdateAsync(ep, ct);
        await uow.SaveChangesAsync(ct);
    }
}
