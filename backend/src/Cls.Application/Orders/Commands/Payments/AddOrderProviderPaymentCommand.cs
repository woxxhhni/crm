using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using MediatR;

namespace Cls.Application.Orders.Commands.Payments;

public record AddOrderProviderPaymentCommand(int OrderId, decimal Amount, string PaymentType, string? Description
    , List<int> FileIds, int UserId) : IRequest;
public class AddOrderProviderPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<AddOrderProviderPaymentCommand>
{
    public async Task Handle(AddOrderProviderPaymentCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.GetByIdAsync(r.OrderId, ct);

        var payment = new ProviderOrderPayment(r.OrderId, r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType, true), r.Description
            , r.FileIds, r.UserId);
        order.AddProviderPayment(payment);

        await uow.Orders.UpdateAsync(order, ct);
        await uow.SaveChangesAsync(ct);
        await logService.ProviderPayment.Added(payment, ct);
    }
}