using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.Orders.Commands;

public record AddOrderSellInvoicesCommand(int OrderId, IReadOnlyCollection<int> FileIds) : IRequest;

public class AddOrderSellInvoicesCommandHandler(IUnitOfWork uow) : IRequestHandler<AddOrderSellInvoicesCommand>
{
    public async Task Handle(AddOrderSellInvoicesCommand r, CancellationToken ct)
    {
        if (r.FileIds is null || r.FileIds.Count == 0)
            return;

        var now = DateTime.UtcNow;

        foreach (var fileId in r.FileIds.Distinct())
        {
            var entity = new OrderSellInvoice
            {
                OrderId = r.OrderId,
                FileId = fileId,
                UploadedAt = now
            };

            await uow.OrderSellInvoices.AddAsync(entity, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
