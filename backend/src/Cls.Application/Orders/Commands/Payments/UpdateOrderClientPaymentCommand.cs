using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Enums;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Payments;

public record UpdateOrderClientPaymentCommand(int OrderId, int Id, decimal Amount, string PaymentType, string? Description 
    , List<int> FileIds, List<int> RemovedFileIds, int UserId) : IRequest;

public class UpdateOrderClientPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<UpdateOrderClientPaymentCommand>
{
    public async Task Handle(UpdateOrderClientPaymentCommand r, CancellationToken ct)
    {
        //TODO: one command handler for client payment: duplicate code
        var order = await uow.Orders.Query()
            .Include(x=> x.ClientPayments)
                .ThenInclude(x=>x.Files)
            .FirstOrDefaultAsync(x=> x.Id == r.OrderId, ct);
        if (order is null)
            throw new NotFoundException("Order not found");

        order.UpdateClientPayment(r.Id, r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType)
        , r.Description, r.FileIds, r.RemovedFileIds, r.UserId);

        await uow.Orders.UpdateAsync(order, ct);
        await logService.ClientPayment.Edited(order.ClientPayments.First(x => x.Id == r.Id), ct);
        await uow.SaveChangesAsync(ct);
    }
}