using Cls.Application.Abstractions;
using MediatR;
using Cls.Domain.Enums;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Payments;

public record UpdateOrderExtraProviderPaymentCommand(int OrderId, int PaymentId, decimal Amount, string PaymentType, string? Description
    , List<int> FileIds, List<int> RemovedFileIds, int UserId) : IRequest;

public class UpdateOrderExtraProviderPaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateOrderExtraProviderPaymentCommand>
{
    public async Task Handle(UpdateOrderExtraProviderPaymentCommand r, CancellationToken ct)
    {
        var payment = await uow.ExtraProviderPayments.Query()
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Id == r.PaymentId && x.OrderId == r.OrderId, ct);

        if (payment == null) throw new NotFoundException("Payment not found");

        payment.Update(r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType, true), r.Description, r.FileIds, r.RemovedFileIds, r.UserId);

        await uow.ExtraProviderPayments.UpdateAsync(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}
