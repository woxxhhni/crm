using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Extensions;
using MediatR;

namespace Cls.Application.OrderLogs.Services;

public sealed class StateOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.IState
{
    public async Task<int> Completed(Order order, DateTime actionDate, string? description, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderCompleted, order, actionDate, description, ct);

    public async Task<int> Canceled(Order order, DateTime actionDate, string? description, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderCanceled, order, actionDate, description, ct);

    public async Task<int> Suspended(Order order, DateTime actionDate, string? description, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderSuspended, order, actionDate, description, ct);

    public async Task<int> ReturnedToProgress(Order order, DateTime actionDate, string? description, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderReturnedToProgress, order, actionDate, description, ct);

    public async Task<int> Created(Order order, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderCreated, order, null, null, ct);

    public async Task<int> Edited(Order order, CancellationToken ct = default)
        => await LogStatus(OrderLogType.OrderEdited, order, null, null, ct);

    private async Task<int> LogStatus(OrderLogType type, Order order, DateTime? ActionDate, string? Description, CancellationToken ct = default)
    {
        var newOrder = await uow.Orders.GetByIdAsync(order.Id, ct);
        if (newOrder == null)
            throw new Exception("Invalid Order");

        var log = new OrderLog()
        {
            LogType = type,
            OrderId = newOrder.Id,
            StepId = newOrder.CurrentStepId,
            Title = $"{type.GetDescription()} for order \"{newOrder.Title}\"",
            Description = string.IsNullOrEmpty(Description)
                ? $"{type.GetDescription()} — order \"{newOrder.Title}\""
                : Description,
            Metadata = json.Serialize(newOrder),
            ActorUserId = currentUser.UserId,
            LogDate = type switch
            {
                OrderLogType.OrderCreated => order.OrderDate,
                OrderLogType.OrderEdited => DateTime.UtcNow,
                _ => ActionDate.HasValue ? ActionDate.Value : DateTime.UtcNow,
            },
            Files = order.StatusFiles.Select(f => new OrderLogFile(f.FileId, currentUser.UserId)).ToList()
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        if (type == OrderLogType.OrderCreated)
            _ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }
}
