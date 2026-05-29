using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using MediatR;
using Cls.Shared.Exceptions;

namespace Cls.Application.Orders.Commands.ExtraProviders;

public record AddOrderExtraProviderCommand(int OrderId, int ProviderId, decimal Amount, string Currency, int UserId) : IRequest<int>;

public class AddOrderExtraProviderCommandHandler(IUnitOfWork uow) : IRequestHandler<AddOrderExtraProviderCommand, int>
{
    public async Task<int> Handle(AddOrderExtraProviderCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.GetByIdAsync(r.OrderId, ct);
        if (order == null) throw new NotFoundException($"Order {r.OrderId} not found");

        var provider = await uow.Providers.GetByIdAsync(r.ProviderId, ct);
        if (provider == null) throw new NotFoundException($"Provider {r.ProviderId} not found");

        var extraProvider = new ExtraProvider
        {
            OrderId = r.OrderId,
            ProviderId = r.ProviderId,
            Amount = r.Amount,
            Currency = r.Currency,
            CreatedByUserId = r.UserId,
            UpdatedByUserId = r.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.ExtraProviders.AddAsync(extraProvider, ct);
        await uow.SaveChangesAsync(ct);
        return extraProvider.Id;
    }
}
