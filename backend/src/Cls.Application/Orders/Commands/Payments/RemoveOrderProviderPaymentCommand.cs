using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Payments;

public record RemoveOrderProviderPaymentCommand(int OrderId, int Id, int UserId) : IRequest;
public class RemoveOrderProviderPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<RemoveOrderProviderPaymentCommand>
{
    public async Task Handle(RemoveOrderProviderPaymentCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.ProviderPayments)
            .FirstOrDefaultAsync(x => x.Id == r.OrderId, ct);
        if (order is null)
            throw new NotFoundException("Order not found");
        var payment = order.ProviderPayments.FirstOrDefault(p => p.Id == r.Id);
        if (payment is null)
            throw new NotFoundException("Payment not found");

        order.RemoveProviderPayment(payment, r.UserId);

        await uow.Orders.UpdateAsync(order, ct);
        await logService.ProviderPayment.Removed(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}