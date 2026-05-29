using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Exceptions;
using Cls.Shared.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Commands;

public record UpdateOrderCommand(int Id, string Title, DateTime OrderDate, string? Description, string BuyCurrency,
                                 decimal BuyAmount, string SellCurrency, decimal SellAmount, int? ClientId, int? ProviderId
                                 , List<int>? RemovedBuyFileIds, List<int>? RemovedSellFileIds, List<int>? EmployeeUserIds) : IRequest<Order?>;

public class UpdateOrderCommandHandler(IUnitOfWork uow, IMapper mapper, IOrderLogService orderLogService, ICurrentUserService currentUserService) : IRequestHandler<UpdateOrderCommand, Order?>
{
    public async Task<Order?> Handle(UpdateOrderCommand r, CancellationToken ct)
    {
        var order = await uow.Orders
            .Query()
            .Include(x=>x.Employees)
            .Include(x=>x.BuyInvoices)
            .Include(x=>x.SellInvoices)
            .FirstOrDefaultAsync(x=> x.Id == r.Id, ct);
        if (order == null)
            throw new NotFoundException();
        if (order.Status != OrderStatus.InProgress)
            throw new InvalidActionException("Order is not in progress you can not change it");
        mapper.Map(r, order);
        order.UpdatedAt = DateTime.UtcNow;
        foreach (var fileId in r.RemovedBuyFileIds ?? new List<int>())
        {
           var file = order.BuyInvoices.First(x => x.FileId == fileId);
           order.BuyInvoices.Remove(file);
        }
        foreach (var fileId in r.RemovedSellFileIds ?? new List<int>())
        {
            var file = order.SellInvoices.First(x => x.FileId == fileId);
            order.SellInvoices.Remove(file);
        }
     
        order.UpdateEmployees(r.EmployeeUserIds, currentUserService.UserId);

        await uow.Orders.UpdateAsync(order, ct);
        await orderLogService.Order.Edited(order, ct);
        await uow.SaveChangesAsync(ct);
        return order;
    }
}
