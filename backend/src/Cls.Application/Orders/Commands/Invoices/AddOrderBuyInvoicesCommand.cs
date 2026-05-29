using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;

namespace Cls.Application.Orders.Commands;

public record AddOrderBuyInvoicesCommand(int OrderId, IReadOnlyCollection<int> FileIds) : IRequest;

public class AddOrderBuyInvoicesCommandHandler(IUnitOfWork uow) : IRequestHandler<AddOrderBuyInvoicesCommand>
{
    public async Task Handle(AddOrderBuyInvoicesCommand r, CancellationToken ct)
    {
        if (r.FileIds is null || r.FileIds.Count == 0)
            return;

        var now = DateTime.UtcNow;

        foreach (var fileId in r.FileIds.Distinct())
        {
            var entity = new OrderBuyInvoice
            {
                OrderId = r.OrderId,
                FileId = fileId,
                UploadedAt = now
            };

            await uow.OrderBuyInvoices.AddAsync(entity, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
