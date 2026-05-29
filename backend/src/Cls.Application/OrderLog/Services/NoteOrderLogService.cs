using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Exceptions;
using Cls.Shared.Extensions;

namespace Cls.Application.OrderLogs.Services;

public sealed class NoteOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.INote
{
    public async Task<int> Added(Note note , CancellationToken ct = default)
        => await LogNote(OrderLogType.NoteAdded, note, ct);

    public async Task<int> Edited(Note note, CancellationToken ct = default)
        => await LogNote(OrderLogType.NoteEdited, note, ct);

    public async Task<int> Removed(Note note, CancellationToken ct = default)
        => await LogNote(OrderLogType.NoteRemoved, note, ct);

    private async Task<int> LogNote(OrderLogType type, Note note, CancellationToken ct = default)
    {
        var order = note.Order;
        if (order is null)
        {
            order = await uow.Orders.GetByIdAsync(note.OrderId, ct);
            if (order == null)
                throw new Exception("Invalid Order");
        }

        var log = new OrderLog()
        {
            LogType = type,
            OrderId = note.OrderId,
            StepId = order.CurrentStepId,
            NoteId = note.Id,
            Title = $"{type.GetDescription()} for Order: {order.Title}",
            Description = $"{type.GetDescription()} for Order: {order.OrderNumber} - {note.Title} ",
            Metadata = json.Serialize(note),
            ActorUserId = currentUser.UserId,
            LogDate = DateTime.UtcNow,
            Files = note.Files.Where(x=> !x.IsDeleted).Select(x => new OrderLogFile(x.FileId, currentUser.UserId)).ToList()
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        if(type == OrderLogType.NoteAdded)
            _ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }
}
