using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Application.Orders.Services;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Abstractions;
using MediatR;

namespace Cls.Application.Orders.Commands.Notes;

public record AddOrderNoteCommand(int OrderId, DateTime Date, string Title, string Description, List<int> FileIds) : IRequest;
public class AddOrderNoteCommandHandler(IUnitOfWork uow, IOrderLogService logService, ICurrentUserService currentUserService) : IRequestHandler<AddOrderNoteCommand>
{
    public async Task Handle(AddOrderNoteCommand r, CancellationToken ct)
    {
        var order = await OrderLoader.GetWithEmployeesAsync(uow, r.OrderId, ct);

        var note = new Note(order.Id, order.CurrentStepId, r.Date.ToUniversalTime(), r.Title, r.Description, r.FileIds, currentUserService.UserId);
        order.AddNote(note, currentUserService);

        await uow.Orders.UpdateAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        await logService.Note.Added(note, ct);
    }
}