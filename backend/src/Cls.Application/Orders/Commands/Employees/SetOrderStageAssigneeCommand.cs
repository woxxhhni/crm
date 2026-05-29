using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands;

public record SetOrderStageAssigneeCommand(int OrderId, int StageId, int? UserId) : IRequest;

public class SetOrderStageAssigneeCommandHandler(IUnitOfWork uow)
    : IRequestHandler<SetOrderStageAssigneeCommand>
{
    public async Task Handle(SetOrderStageAssigneeCommand request, CancellationToken ct)
    {
        var orderExists = await uow.Orders.Query().AnyAsync(x => x.Id == request.OrderId, ct);
        if (!orderExists)
            throw new NotFoundException("Order not found");

        var stageExists = await uow.Stages.Query().AnyAsync(x => x.Id == request.StageId && x.IsActive, ct);
        if (!stageExists)
            throw new NotFoundException("Stage not found");

        if (request.UserId.HasValue)
        {
            var userExists = await uow.Users.Query().AnyAsync(x => x.Id == request.UserId.Value && x.IsActive, ct);
            if (!userExists)
                throw new NotFoundException("Employee not found");
        }

        var existing = await uow.OrderStageAssignments.Query()
            .FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.StageId == request.StageId, ct);

        if (!request.UserId.HasValue)
        {
            if (existing is not null)
                await uow.OrderStageAssignments.DeleteAsync(existing, ct);

            await uow.SaveChangesAsync(ct);
            return;
        }

        if (existing is null)
        {
            await uow.OrderStageAssignments.AddAsync(new OrderStageAssignment
            {
                OrderId = request.OrderId,
                StageId = request.StageId,
                UserId = request.UserId.Value
            }, ct);
        }
        else
        {
            existing.UserId = request.UserId.Value;
            await uow.OrderStageAssignments.UpdateAsync(existing, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
