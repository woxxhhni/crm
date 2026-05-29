using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ex = Cls.Shared.Exceptions;

namespace Cls.Application.Orders.Commands;

public record CompleteOrderCommand(int Id, DateTime ActionDate, string? Description, List<int> FileIds, List<int> RemovedFileIds) : IRequest;
public record CancelOrderCommand(int Id, DateTime ActionDate, string? Description, List<int> FileIds, List<int> RemovedFileIds) : IRequest;
public record SuspendOrderCommand(int Id, DateTime ActionDate, string? Description, List<int> FileIds, List<int> RemovedFileIds) : IRequest;
public record UnSuspendOrderCommand(int Id, DateTime ActionDate, string? Description, List<int> FileIds, List<int> RemovedFileIds) : IRequest;

public class OrderStatusCommandHandler(IUnitOfWork uow, ICurrentUserService currentUserService, IOrderLogService logService)
    : IRequestHandler<CancelOrderCommand>
      , IRequestHandler<CompleteOrderCommand>
      , IRequestHandler<SuspendOrderCommand>
      , IRequestHandler<UnSuspendOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await GetOrder(request.Id, cancellationToken);
        order.Cancel(request.ActionDate.ToUniversalTime(), request.FileIds, request.RemovedFileIds, currentUserService);
        var lastStepHistory = await uow.OrderStepHistories.Query()
                                           .Where(p => p.OrderId == order.Id)
                                           .OrderByDescending(p => p.CreatedAt)
                                           .FirstOrDefaultAsync();
        if (lastStepHistory == null)
            throw new Ex.InvalidActionException("Order Step History Not Found");
        lastStepHistory.ExitedAt = DateTime.UtcNow;
        lastStepHistory.ExitReason = OrderStepExitReason.OrderTerminated;
        await uow.OrderStepHistories.UpdateAsync(lastStepHistory);
        await uow.Orders.UpdateAsync(order, cancellationToken);
        await logService.Order.Canceled(order, request.ActionDate.ToUniversalTime(), request.Description, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await GetOrder(request.Id, cancellationToken);
        if (!order.CurrentStep.IsFinalStep)
            throw new InvalidActionException("Order is not at Final Step");
        order.Complete(request.ActionDate.ToUniversalTime(), request.FileIds, request.RemovedFileIds, currentUserService.UserId);
        await uow.Orders.UpdateAsync(order, cancellationToken);
        await logService.Order.Completed(order, request.ActionDate.ToUniversalTime(), request.Description, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(SuspendOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await GetOrder(request.Id, cancellationToken);
        order.Suspend(request.ActionDate.ToUniversalTime(), request.FileIds, request.RemovedFileIds, currentUserService.UserId);
        await uow.Orders.UpdateAsync(order, cancellationToken);
        await logService.Order.Suspended(order, request.ActionDate.ToUniversalTime(), request.Description, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(UnSuspendOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await GetOrder(request.Id, cancellationToken);
        order.UnSuspended(request.FileIds, request.RemovedFileIds, currentUserService.UserId);
        await uow.Orders.UpdateAsync(order, cancellationToken);
        await logService.Order.ReturnedToProgress(order, request.ActionDate.ToUniversalTime(), request.Description, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order> GetOrder(int id, CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.StatusFiles)
            .Include(x => x.CurrentStep)
            .Include(x => x.Employees.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
            throw new NotFoundException();
        return order;
    }

}
