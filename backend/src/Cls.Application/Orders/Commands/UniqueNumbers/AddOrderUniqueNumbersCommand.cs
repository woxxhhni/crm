using Cls.Application.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands;

public record AddOrderUniqueNumbersCommand(
    int OrderId,
    IReadOnlyCollection<OrderUniqueNumberInput> Items,
    int CreatedByUserId
) : IRequest;

public class AddOrderUniqueNumbersCommandHandler(IUnitOfWork uow, IOrderLogService orderLogService, ICurrentUserService currentUserService)
    : IRequestHandler<AddOrderUniqueNumbersCommand>
{
    public async Task Handle(AddOrderUniqueNumbersCommand r, CancellationToken ct)
    {
        if (r.Items == null || r.Items.Count == 0)
            return;

        var existOrder = await OrderLoader.GetWithEmployeesAsync(uow, r.OrderId, ct);
        if (currentUserService.Role == UserRole.Employee && !OrderAccess.IsAssignedToUser(existOrder, currentUserService.UserId))
            throw new ForbiddenException("Invalid access");

        var now = DateTime.UtcNow;

        var labels = r.Items.Select(x => x.Label.Trim()).ToArray();

        var existing = await uow.OrderUniqueNumbers
            .Query()
            .Where(x => x.OrderId == r.OrderId && labels.Contains(x.Label))
            .ToListAsync(ct);

        var existingByLabel = existing.ToDictionary(x => x.Label, StringComparer.OrdinalIgnoreCase);

        foreach (var item in r.Items)
        {
            var label = item.Label.Trim();
            var value = item.Value.Trim();

            if (existingByLabel.TryGetValue(label, out var entity))
            {
                entity.Value = value;
                entity.UpdatedAt = now;
                await uow.OrderUniqueNumbers.UpdateAsync(entity, ct);
            }
            else
            {
                var newEntity = new OrderUniqueNumber
                {
                    OrderId = r.OrderId,
                    Label = label,
                    Value = value,
                    CreatedAt = now,
                    CreatedByUserId = r.CreatedByUserId
                };
                await uow.OrderUniqueNumbers.AddAsync(newEntity, ct);
                await orderLogService.UniqueNumber.Added(newEntity, ct);
            }
        }

        await uow.SaveChangesAsync(ct);
    }
}
