using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Extensions;

namespace Cls.Application.OrderLogs.Services;

public sealed class UniqueNumberOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.IUniqueNumber
{
    public async Task<int> Added(OrderUniqueNumber uniqueNumber, CancellationToken ct = default)
        => await LogUniqueNumber(OrderLogType.UniqueNumberAdded, uniqueNumber, ct);

    public async Task<int> Removed(OrderUniqueNumber uniqueNumber, CancellationToken ct = default)
        => await LogUniqueNumber(OrderLogType.UniqueNumberRemoved, uniqueNumber, ct);

    private async Task<int> LogUniqueNumber(OrderLogType type, OrderUniqueNumber uniqueNumber, CancellationToken ct = default)
    {
        var order = await uow.Orders.GetByIdAsync(uniqueNumber.OrderId, ct);
        if (order == null)
            throw new Exception("Invalid Order");

        var log = new OrderLog()
        {
            LogType = type,
            OrderId = uniqueNumber.OrderId,
            StepId = order.CurrentStepId,
            Title = $"{type.GetDescription()} for Order: {order.Title}",
            Description = $"{type.GetDescription()} for Order: {order.Title} - Unique Number: {uniqueNumber.Label} ", 
            Metadata = json.Serialize(uniqueNumber),
            ActorUserId = currentUser.UserId,
            LogDate = DateTime.UtcNow
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        return log.Id;
    }
}
