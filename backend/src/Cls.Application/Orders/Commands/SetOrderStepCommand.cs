using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands;

public record SetOrderStepCommand(int OrderId, int StepId, DateTime ActionDate, string? Description, List<int> FileIds) : IRequest<Order?>;

public class SetOrderStepCommandHandler(IUnitOfWork uow, IOrderLogService logService, ICurrentUserService currentUserService) : IRequestHandler<SetOrderStepCommand, Order?>
{
    public async Task<Order?> Handle(SetOrderStepCommand req, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
                                    .Include(p => p.CurrentStep)
                                    .Include(p => p.Employees)
                                    .Include(p => p.StageAssignments)
                                    .FirstOrDefaultAsync(p => p.Id == req.OrderId, ct);
        if (order is null)
            throw new NotFoundException("Order Not Found");
        if (currentUserService.Role == UserRole.Employee)
            if (!OrderAccess.IsAssignedToUser(order, currentUserService.UserId))
                throw new InvalidActionException("Employee can't set order's step to completed");

        if (order.Status != OrderStatus.InProgress)
            throw new InvalidActionException("Order Step change is not permitted");
        var fromStep = order.CurrentStep;

        var stepHistory = await uow.OrderStepHistories.Query()
                                           .Where(p => p.OrderId == order.Id)
                                           .OrderByDescending(p => p.CreatedAt)
                                           .FirstOrDefaultAsync(ct);
        if (stepHistory is null)
            throw new InvalidActionException("There is no Step History for Current Step");

        var toStep = await uow.Steps.Query().FirstOrDefaultAsync(p => p.Id == req.StepId, ct);
        if (toStep is null)
            throw new InvalidActionException("Selected StepId is Invalid");

        if (fromStep.Id == toStep.Id)
            throw new InvalidActionException("Selected Step already assigned to order");

        order.CurrentStepId = req.StepId;
        var newStepHistory = new OrderStepHistory()
        {
            OrderId = order.Id,
            StepId = order.CurrentStepId,
            EnteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserService.UserId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserId = currentUserService.UserId,
            IsDeleted = false
        };

        stepHistory.ExitedAt = DateTime.UtcNow;
        if (toStep.OrderPosition > order.CurrentStep.OrderPosition)
        {
            stepHistory.ExitReason = OrderStepExitReason.MovedForward;
            newStepHistory.EntryType = OrderStepEntryType.Forward;
            await logService.Status.Forward(order, fromStep, toStep, req.ActionDate.ToUniversalTime(), req.Description, req.FileIds, ct);
        }
        else if (toStep.OrderPosition < order.CurrentStep.OrderPosition)
        {
            stepHistory.ExitReason = OrderStepExitReason.MovedBackward;
            newStepHistory.EntryType = OrderStepEntryType.Backward;
            await logService.Status.Backward(order, fromStep, toStep, req.ActionDate.ToUniversalTime(), req.Description, req.FileIds, ct);
        }
        await uow.OrderStepHistories.UpdateAsync(stepHistory);
        order.AddStepHistory(newStepHistory);
        await uow.Orders.UpdateAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        return order;
    }
}
