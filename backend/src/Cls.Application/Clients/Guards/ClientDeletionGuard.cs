using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Clients.Guards;

public class ClientDeletionGuard(IUnitOfWork uow) : IDeletionGuard<Client>
{
    public async Task EnsureCanDeleteAsync(Client client, CancellationToken ct = default)
    {
        var hasActiveOrders = await uow.Orders
                                       .Query()
                                       .AnyAsync(o => o.ClientId == client.Id && !o.IsDeleted, ct);

        if (hasActiveOrders)
            throw new InvalidActionException("Client cannot be deleted because there are active orders.");
    }
}
