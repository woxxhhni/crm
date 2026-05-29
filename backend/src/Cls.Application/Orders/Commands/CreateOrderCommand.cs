
using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Orders;
using Cls.Shared.Mapping;
using MediatR;

namespace Cls.Application.Orders.Commands;

public record CreateOrderCommand(string Title, DateTime OrderDate, string? Description, string BuyCurrency, 
                                 decimal BuyAmount, string SellCurrency, decimal SellAmount, int? ClientId, int? ProviderId, 
                                 int? CurrentStepId, int? CreatedByUserId) : IRequest<Order>;

public class CreateOrderCommandHandler(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUserService, IOrderLogService orderLogService) : IRequestHandler<CreateOrderCommand, Order>
{
    public async Task<Order> Handle(CreateOrderCommand r, CancellationToken ct)
    {
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var entity = mapper.Map<Order>(r);
        entity.OrderNumber = orderNumber;
        entity.AddStepHistory(new OrderStepHistory()
        {
            OrderId = entity.Id,
            StepId = entity.CurrentStepId,
            EnteredAt = DateTime.UtcNow,
            EntryType = OrderStepEntryType.Initial, 
            CreatedAt = DateTime.UtcNow, 
            CreatedByUserId = currentUserService.UserId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserId = currentUserService.UserId, 
            IsDeleted = false
        });
        entity = await uow.Orders.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        await orderLogService.Order.Created(entity, ct);
        return entity;
    }
}