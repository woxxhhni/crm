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

public record SetOrderStepCompleteCommand(int OrderId, DateTime ActionDate, string? Description, List<int> FileIds) : IRequest<Order?>;

public class SetOrderStepCompleteCommandHandler(IUnitOfWork uow, IOrderLogService log, ICurrentUserService currentUserService) : IRequestHandler<SetOrderStepCompleteCommand, Order?>
{
    public async Task<Order?> Handle(SetOrderStepCompleteCommand req, CancellationToken ct)
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
                throw new InvalidActionException("Employee can't change this order's step");

        if (order.Status != OrderStatus.InProgress)
            throw new InvalidActionException("Order Step change is not permitted");
        var prevStep = order.CurrentStep;

        var stepHistory = await uow.OrderStepHistories.Query()
                                           .Where(p => p.OrderId == order.Id)
                                           .OrderByDescending(p => p.CreatedAt)
                                           .FirstOrDefaultAsync();
        if (stepHistory is null)
            throw new InvalidActionException("There is no Step History for Current Step");

        if (stepHistory.ExitedAt.HasValue)
            throw new InvalidActionException("Step has been completed already");

        var nextStep = uow.Steps.Query()
                                .OrderBy(p => p.OrderPosition)
                                .Where(p => p.OrderPosition > order.CurrentStep.OrderPosition)
                                .FirstOrDefault();
        if (nextStep is null && !prevStep.IsFinalStep)
            throw new InvalidActionException("Step completion failed, There is no Next Step");

        await log.Status.Complete(order, prevStep, req.ActionDate.ToUniversalTime(), req.Description, req.FileIds, ct);
        if (nextStep is not null)
        {
            order.CurrentStepId = nextStep.Id;
            var newStepHistory = new OrderStepHistory()
            {
                OrderId = order.Id,
                StepId = order.CurrentStepId,
                EntryType = OrderStepEntryType.Forward,
                EnteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserService.UserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserService.UserId,
                IsDeleted = false
            };
            order.AddStepHistory(newStepHistory);
            await log.Status.Forward(order, prevStep, nextStep, req.ActionDate.ToUniversalTime(), req.Description, req.FileIds, ct);
        }

        stepHistory.ExitedAt = DateTime.UtcNow;
        stepHistory.ExitReason = OrderStepExitReason.Completed;
        await uow.OrderStepHistories.UpdateAsync(stepHistory);
        await uow.Orders.UpdateAsync(order, ct);

        await uow.SaveChangesAsync(ct);
        return order;
    }
}
