using MediatR;
using Cls.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Cls.Application.OrderLogs.Abstractions;

namespace Cls.Application.Orders.Commands;

public record RemoveOrderUniqueNumberCommand(
    int OrderId,
    string Label
) : IRequest;

public class RemoveOrderUniqueNumberCommandHandler(IUnitOfWork uow, IOrderLogService orderLogService)
    : IRequestHandler<RemoveOrderUniqueNumberCommand>
{
    public async Task Handle(RemoveOrderUniqueNumberCommand r, CancellationToken ct)
    {
        var label = r.Label.Trim();

        var entities = await uow.OrderUniqueNumbers
            .Query()
            .Where(x => x.OrderId == r.OrderId && x.Label == label)
            .ToListAsync(ct);

        if (entities.Count == 0) return;

        foreach (var e in entities)
        {
            await uow.OrderUniqueNumbers.DeleteAsync(e, ct);
            await orderLogService.UniqueNumber.Removed(e, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
