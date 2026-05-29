using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.OrderLogs.Services;

public sealed class EmployeeOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.IEmployee
{
    public async Task<int> Assigned(OrderEmployee employee, CancellationToken ct = default)
        => await LogEmployeeOrder(OrderLogType.EmployeeAssigned, employee, ct);

    public async Task<int> Removed(OrderEmployee employee, CancellationToken ct = default)
        => await LogEmployeeOrder(OrderLogType.EmployeeRemoved, employee, ct);

    private async Task<int> LogEmployeeOrder(OrderLogType type, OrderEmployee employee, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetByIdAsync(employee.OrderId, ct)
            ?? throw new InvalidOperationException("Invalid Order");

        var assignedUser = await uow.Users.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == employee.UserId, ct);
        var employeeName = assignedUser?.Name ?? $"User #{employee.UserId}";

        var log = new OrderLog()
        {
            LogType = type,
            OrderId = employee.OrderId,
            StepId = order.CurrentStepId,
            Title = $"{type.GetDescription()}: {employeeName}",
            Description = $"{type.GetDescription()} for order \"{order.Title}\" — {employeeName}",
            Metadata = json.Serialize(employee),
            ActorUserId = currentUser.UserId,
            LogDate = DateTime.UtcNow
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        if(type == OrderLogType.EmployeeAssigned)
            _ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }
}
