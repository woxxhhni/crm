using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Cls.Shared.Contracts.Orders.OrderSummaryResponse;

namespace Cls.Application.Orders.Queries;

public record GetOrderSummaryQuery() : IRequest<OrderSummaryResponse>;
public class GetOrderSummaryQueryHandler(IUnitOfWork uow) : IRequestHandler<GetOrderSummaryQuery, OrderSummaryResponse>
{
    public async Task<OrderSummaryResponse> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders.Query()
            .GroupBy(x => x.Status)
            .Select(x => new
            {
                Status = x.Key,
                Count = x.Count()
            }).ToListAsync(cancellationToken);

        var result =  new OrderSummaryResponse
        {
            InProgress = order.FirstOrDefault(x => x.Status == OrderStatus.InProgress)?.Count ?? 0,
            Completed = order.FirstOrDefault(x => x.Status == OrderStatus.Completed)?.Count ?? 0,
            Canceled = order.FirstOrDefault(x => x.Status == OrderStatus.Canceled)?.Count ?? 0,
            Suspended = order.FirstOrDefault(x => x.Status == OrderStatus.Suspended)?.Count ?? 0,
        };

        var stages = await uow.Stages.Query()
            .ToListAsync(cancellationToken);

        var totalOrderCount = await uow.Orders.Query().CountAsync(cancellationToken);
        foreach (var stage in stages)
        {
            var currentStageCount = await uow.Orders.Query().Include(x => x.CurrentStep)
                .CountAsync(x => x.CurrentStep.StageId == stage.Id, cancellationToken);
            result.StageSummaries.Add(new OrderStageSummaryResponse
            {
                Name = stage.Name,
                Percent = totalOrderCount == 0
                    ? 0
                    : Math.Round(currentStageCount / (decimal)totalOrderCount * 100, 0)
            });
        }

        return result;
    }
}
