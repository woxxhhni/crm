using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Payments;

public record RemoveOrderClientPaymentCommand(int OrderId, int Id, int UserId) : IRequest;

public class RemoveOrderClientPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<RemoveOrderClientPaymentCommand>
{
    public async Task Handle(RemoveOrderClientPaymentCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.ClientPayments)
            .FirstOrDefaultAsync(x => x.Id == r.OrderId, ct);
        if (order is null)
            throw new NotFoundException("Order not found");
        var payment = order.ClientPayments.FirstOrDefault(p => p.Id == r.Id);
        if (payment is null)
            throw new NotFoundException("Payment not found");

        order.RemoveClientPayment(payment, r.UserId);
        await uow.Orders.UpdateAsync(order, ct);
        await logService.ClientPayment.Removed(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}