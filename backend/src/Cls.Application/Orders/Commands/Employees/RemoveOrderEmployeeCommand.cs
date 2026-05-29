using MediatR;
using Cls.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Shared.Contracts.Abstractions;

namespace Cls.Application.Orders.Commands;

public record RemoveOrderEmployeeCommand(int OrderId, int UserId) : IRequest;

public class RemoveOrderEmployeeCommandHandler(IUnitOfWork uow, IOrderLogService orderLogService, ICurrentUserService currentUserService)
    : IRequestHandler<RemoveOrderEmployeeCommand>
{
    public async Task Handle(RemoveOrderEmployeeCommand r, CancellationToken ct)
    {
        var existing = await uow.OrderEmployees
            .Query()
            .Where(x => x.OrderId == r.OrderId &&
                        x.UserId == r.UserId)
            .ToListAsync(ct);

        if (existing.Count == 0) return;

        var now = DateTime.UtcNow;

        foreach (var e in existing)
        {
            e.IsDeleted = true;
            e.DeletedAt = now;
            e.DeletedByUserId = currentUserService.UserId;
            await uow.OrderEmployees.UpdateAsync(e, ct);
            await orderLogService.Employee.Removed(e, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
