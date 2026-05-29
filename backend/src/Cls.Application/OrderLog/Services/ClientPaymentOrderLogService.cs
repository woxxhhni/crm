using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Extensions;

namespace Cls.Application.OrderLogs.Services;

public sealed class ClientPaymentOrderLogService(
                    IUnitOfWork uow, 
                    ICurrentUserService currentUser, 
                    IJsonSerializer json) : IOrderLogService.IClientPayment
{
    public async Task<int> Added(ClientOrderPayment payment, CancellationToken ct = default)
        => await LogClientPayment(OrderLogType.ClientPaymentAdded, payment, ct);

    public async Task<int> Edited(ClientOrderPayment payment, CancellationToken ct = default)
        => await LogClientPayment(OrderLogType.ClientPaymentEdited, payment, ct);

    public async Task<int> Removed(ClientOrderPayment payment, CancellationToken ct = default)
        => await LogClientPayment(OrderLogType.ClientPaymentRemoved, payment, ct);

    private async Task<int> LogClientPayment(OrderLogType type, ClientOrderPayment payment, CancellationToken ct = default)
    {
        var order = payment.Order;
        if (order is null)
        {
            order = await uow.Orders.GetByIdAsync(payment.OrderId, ct);
            if (order == null)
                throw new Exception("Invalid Order");
        }

        var log = new OrderLog()
        {
            LogType = type,
            OrderId = payment.OrderId,
            StepId = order.CurrentStepId,
            Title = $"{type.GetDescription()} for OrderId: {payment.OrderId}",
            Description = $"{type.GetDescription()} for OrderId: {payment.OrderId} - Payment  [Date: {payment.PaymentDate} - Type: {payment.PaymentType} - Amount: {payment.Amount}]",
            Metadata = json.Serialize(payment),
            ActorUserId = currentUser.UserId,
            LogDate = DateTime.UtcNow,
            Files = payment.Files.Where(x => !x.IsDeleted).Select(x => new OrderLogFile(x.FileId, currentUser.UserId)).ToList()
        };
        _ = await uow.OrderLogs.AddAsync(log, ct);
        if(type == OrderLogType.ClientPaymentAdded)
            _ = await uow.SaveChangesAsync(ct);

        return log.Id;
    }
}
