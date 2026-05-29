using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Providers.Guards;

public class ProviderDeletionGuard(IUnitOfWork uow) : IDeletionGuard<Provider>
{
    public async Task EnsureCanDeleteAsync(Provider provider, CancellationToken ct = default)
    {
        var hasActiveOrders = await uow.Orders
                                       .Query()
                                       .AnyAsync(o => o.ProviderId == provider.Id && !o.IsDeleted, ct);

        if (hasActiveOrders)
            throw new InvalidActionException("Provider cannot be deleted because there are active orders.");
    }
}
