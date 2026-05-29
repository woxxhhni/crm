using MediatR;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record GetOrderByIdQuery(int Id) : IRequest<Order?>;

public class GetOrderByIdHandler(IUnitOfWork uow) : IRequestHandler<GetOrderByIdQuery, Order?>
{
    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await uow.Orders
                             .Query()
                             .Include(p => p.Client)
                             .Include(p => p.Provider)
                             .Include(p => p.ExtraProviders).ThenInclude(p => p.Provider)
                             .Include(p => p.ExtraProviders).ThenInclude(p => p.Payments)
                             .Include(p => p.SellInvoices)
                             .Include(p => p.BuyInvoices)
                             .Include(p => p.UniqueNumbers)
                             .Include(p => p.CurrentStep).ThenInclude(p => p.Stage)
                             .Include(p => p.Employees.Where(e => !e.IsDeleted)).ThenInclude(p => p.User)
                             .Include(p => p.StageAssignments.Where(e => !e.IsDeleted)).ThenInclude(p => p.User)
                             .Include(p => p.Logs)
                                 .ThenInclude(p => p.Step).ThenInclude(p => p.Stage)
                             .Include(p => p.Logs)
                                 .ThenInclude(p => p.Files).ThenInclude(p => p.File)
                             .Include(x => x.Logs)
                                 .ThenInclude(x => x.ActorUser)
                             .Include(x => x.Logs)
                                 .ThenInclude(x => x.FromStep)
                             .Include(x => x.Logs)
                                 .ThenInclude(x => x.ToStep)
                             .Include(x => x.Notes)
                                 .ThenInclude(x => x.Files).ThenInclude(x => x.File)
                             .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (order == null)
            throw new NotFoundException();

        order.Logs = order.Logs
            .OrderByDescending(l => l.CreatedAt)
            .ToList();

        return order;
    }
}
