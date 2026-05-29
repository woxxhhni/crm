using Cls.Application.Abstractions;
using MediatR;
using Cls.Shared.Exceptions;

namespace Cls.Application.Orders.Commands.Payments;

public record RemoveOrderExtraProviderPaymentCommand(int OrderId, int PaymentId, int UserId) : IRequest;

public class RemoveOrderExtraProviderPaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<RemoveOrderExtraProviderPaymentCommand>
{
    public async Task Handle(RemoveOrderExtraProviderPaymentCommand r, CancellationToken ct)
    {
        var payment = await uow.ExtraProviderPayments.GetByIdAsync(r.PaymentId, ct);
        if (payment == null) throw new NotFoundException("Payment not found");
        if (payment.OrderId != r.OrderId) throw new InvalidActionException("Payment order mismatch");

        payment.Remove(r.UserId);

        await uow.ExtraProviderPayments.UpdateAsync(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}
