using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Extensions;

namespace Cls.Application.OrderLogs.Services;

public sealed class StatusOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.IStatus
{
    public async Task<int> Forward(Order order, Step fromStep, Step toStep, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default)
        => await LogStatus(OrderLogType.StatusForward, order, fromStep, toStep, actionDate, description, fileIds, ct);

    public async Task<int> Backward(Order order, Step fromStep, Step toStep, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default)
        => await LogStatus(OrderLogType.StatusBackward, order, fromStep, toStep, actionDate, description, fileIds, ct);

    public async Task<int> Complete(Order order, Step step, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default)
    {
        var log = new OrderLog()
        {
            LogType = OrderLogType.StatusComplete,
            OrderId = order.Id,
            StepId = step.Id,
            Title = $"Step completed: {step.Name}",
            Description = string.IsNullOrEmpty(description)
                ? $"Step \"{step.Name}\" marked complete for order \"{order.Title}\""
                : description,
            Metadata = json.Serialize(order),
            ActorUserId = currentUser.UserId,
            LogDate = actionDate,
            Files = fileIds.Select(id => new OrderLogFile(id, currentUser.UserId)).ToList()
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        //_ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }

    private async Task<int> LogStatus(OrderLogType type, Order order, Step fromStep, Step toStep, DateTime actionDate, string? description, List<int> fileIds, CancellationToken ct = default)
    {
        var log = new OrderLog()
        {
            LogType = type,
            OrderId = order.Id,
            StepId = order.CurrentStepId,
            FromStepId = fromStep.Id, 
            ToStepId = toStep.Id,
            Title = $"Moved to step: {toStep.Name}",
            Description = string.IsNullOrEmpty(description)
                ? $"{type.GetDescription()} — from \"{fromStep.Name}\" to \"{toStep.Name}\""
                : description,
            Metadata = json.Serialize(order),
            ActorUserId = currentUser.UserId,
            LogDate = actionDate,
            Files = fileIds.Select(id => new OrderLogFile(id , currentUser.UserId)).ToList()
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        //_ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }
}
