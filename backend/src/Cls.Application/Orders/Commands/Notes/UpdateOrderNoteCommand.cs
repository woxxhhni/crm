using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Notes;

public record UpdateOrderNoteCommand(int OrderId, int Id, DateTime Date, string Title, string Description, List<int> FileIds, List<int> RemovedFileIds) : IRequest;
public class UpdateOrderNoteCommandHandler(IUnitOfWork uow, IOrderLogService logService, ICurrentUserService currentUserService) : IRequestHandler<UpdateOrderNoteCommand>
{
    public async Task Handle(UpdateOrderNoteCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.Employees.Where(e => !e.IsDeleted))
            .Include(x => x.StageAssignments.Where(e => !e.IsDeleted))
            .Include(x=>x.Notes)
            .ThenInclude(x=>x.Files)
            .FirstOrDefaultAsync(x=> x.Id == r.OrderId, ct);
        if (order is null)
            throw new NotFoundException();

        order.UpdateNote(r.Id, r.Date.ToUniversalTime(), r.Title, r.Description, r.FileIds, r.RemovedFileIds, currentUserService);

        await uow.Orders.UpdateAsync(order, ct);
        await logService.Note.Edited(order.Notes.First(x => x.Id == r.Id), ct);
        await uow.SaveChangesAsync(ct);
    }
}
