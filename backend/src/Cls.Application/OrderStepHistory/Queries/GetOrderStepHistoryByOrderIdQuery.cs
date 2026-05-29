using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Cls.Application.Steps.Queries;

public record GetOrderStepHistoryByOrderIdQuery(int OrderId) : IRequest<IReadOnlyList<OrderStepHistory>>;
public class GetOrderStepHistoryByOrderIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetOrderStepHistoryByOrderIdQuery, IReadOnlyList<OrderStepHistory>>
{
    public async Task<IReadOnlyList<OrderStepHistory>> Handle(GetOrderStepHistoryByOrderIdQuery req, CancellationToken ct)
        => await uow.OrderStepHistories.Query()
                                       .Include(h => h.Step).ThenInclude(s => s.Stage)
                                       .Where(h => h.OrderId == req.OrderId)
                                       .OrderBy(h => h.EnteredAt)
                                       .ToListAsync(ct);
}
