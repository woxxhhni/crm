using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Enums;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands.Payments;

public record UpdateOrderProviderPaymentCommand(int OrderId, int Id, decimal Amount, string PaymentType, string? Description
    , List<int> FileIds, List<int> RemovedFileIds, int UserId) : IRequest;
public class UpdateOrderProviderPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<UpdateOrderProviderPaymentCommand>
{
    public async Task Handle(UpdateOrderProviderPaymentCommand r, CancellationToken ct)
    {
        var order = await uow.Orders.Query()
            .Include(x => x.ProviderPayments)
                .ThenInclude(x => x.Files)
            .FirstOrDefaultAsync(x => x.Id == r.OrderId, ct);
        if (order is null)
            throw new NotFoundException("Order not found");

        order.UpdateProviderPayment(r.Id, r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType)
            , r.Description, r.FileIds, r.RemovedFileIds, r.UserId);

        await uow.Orders.UpdateAsync(order, ct);
        await logService.ProviderPayment.Edited(order.ProviderPayments.First(x => x.Id == r.Id), ct);
        await uow.SaveChangesAsync(ct);
    }
}