using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands;

public record AssignOrderEmployeesCommand(int OrderId, IList<int> UserIds) : IRequest;

public class AssignOrderEmployeesCommandHandler(IUnitOfWork uow, IOrderLogService orderLogService)
    : IRequestHandler<AssignOrderEmployeesCommand>
{
    public async Task Handle(AssignOrderEmployeesCommand r, CancellationToken ct)
    {
        if (r.UserIds == null || r.UserIds.Count == 0)
            return;

        var existing = await uow.OrderEmployees
            .Query()
            .Where(x => x.OrderId == r.OrderId)
            .ToListAsync(ct);

        var existingUserIds = existing.Select(x => x.UserId).ToHashSet();

        foreach (var userId in r.UserIds.Distinct())
        {
            if (existingUserIds.Contains(userId))
                continue;

            var e = new OrderEmployee
            {
                OrderId = r.OrderId,
                UserId = userId,
            };

            await uow.OrderEmployees.AddAsync(e, ct);
            await orderLogService.Employee.Assigned(e, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
