using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using MediatR;
using Cls.Shared.Exceptions;
using Cls.Domain.Enums;

namespace Cls.Application.Orders.Commands.Payments;

public record AddOrderExtraProviderPaymentCommand(int OrderId, int ExtraProviderId, decimal Amount, string PaymentType, string? Description
    , List<int> FileIds, int UserId) : IRequest;

public class AddOrderExtraProviderPaymentCommandHandler(IUnitOfWork uow) : IRequestHandler<AddOrderExtraProviderPaymentCommand>
{
    public async Task Handle(AddOrderExtraProviderPaymentCommand r, CancellationToken ct)
    {
        var ep = await uow.ExtraProviders.GetByIdAsync(r.ExtraProviderId, ct);
        if (ep == null) throw new NotFoundException("Extra Provider not found");
        if (ep.OrderId != r.OrderId) throw new InvalidActionException("Extra Provider mismatch");

        var payment = new ExtraProviderOrderPayment(r.ExtraProviderId, r.OrderId, r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType, true), r.Description, r.FileIds, r.UserId);

        await uow.ExtraProviderPayments.AddAsync(payment, ct);
        await uow.SaveChangesAsync(ct);
    }
}
