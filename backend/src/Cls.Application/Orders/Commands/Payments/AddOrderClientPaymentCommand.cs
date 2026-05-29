using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cls.Application.Orders.Commands.Payments
{
    public record AddOrderClientPaymentCommand(int OrderId, decimal Amount, string PaymentType, string? Description
    , List<int> FileIds, int UserId) : IRequest;

    public class AddOrderClientPaymentCommandHandler(IUnitOfWork uow, IOrderLogService logService) : IRequestHandler<AddOrderClientPaymentCommand>
    {
        public async Task Handle(AddOrderClientPaymentCommand r, CancellationToken ct)
        {
            var order = await uow.Orders.GetByIdAsync(r.OrderId, ct);

            var payment = new ClientOrderPayment(r.OrderId, r.Amount, Enum.Parse<OrderPaymentType>(r.PaymentType, true), r.Description
                , r.FileIds, r.UserId);
            order.AddClientPayment(payment);

            await uow.Orders.UpdateAsync(order, ct);
            await logService.ClientPayment.Added(payment, ct);
            await uow.SaveChangesAsync(ct);
        }
    }
}
