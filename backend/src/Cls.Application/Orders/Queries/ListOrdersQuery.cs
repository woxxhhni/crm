using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Extensions;
using Cls.Domain.Entities;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Common;
using Cls.Shared.Contracts.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Orders.Queries;

public record ListOrdersQuery(
    int[]? ClientIds,
    int[]? ProviderIds,
    OrderStatus[]? Statuses,
    int[]? StepIds,
    int? EmployeeId,
    DateTime? FromDate,
    DateTime? ToDate,
    decimal? FromAmount,
    decimal? ToAmount,
    PagedRequest? Paging = null) : IRequest<PagedResult<OrderResponse>>;

public class ListOrdersQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<ListOrdersQuery, PagedResult<OrderResponse>>
{
    public async Task<PagedResult<OrderResponse>> Handle(ListOrdersQuery request, CancellationToken ct)
    {
        var q = uow.Orders.Query()
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .AsNoTracking();

        if (request.ClientIds?.Length > 0)
            q = q.Where(o => request.ClientIds.Contains(o.ClientId));

        if (request.ProviderIds?.Length > 0)
            q = q.Where(o => request.ProviderIds.Contains(o.ProviderId));

        if (request.Statuses?.Length > 0)
            q = q.Where(o => request.Statuses.Contains(o.Status));

        if (request.StepIds?.Length > 0)
            q = q.Where(o => request.StepIds.Contains(o.CurrentStepId));

        if (request.FromDate.HasValue)
            q = q.Where(o => o.OrderDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            q = q.Where(o => o.OrderDate <= request.ToDate.Value);

        if (request.FromAmount.HasValue)
            q = q.Where(o => o.SellAmount >= request.FromAmount.Value);

        if (request.ToAmount.HasValue)
            q = q.Where(o => o.SellAmount <= request.ToAmount.Value);

        if (request.EmployeeId.HasValue)
            q = q.Where(o =>
                o.Employees.Any(e => e.UserId == request.EmployeeId.Value) ||
                o.StageAssignments.Any(e => e.UserId == request.EmployeeId.Value));

        var paged = await q.ToPagedResultAsync(
            request.Paging ?? new PagedRequest(),
            defaultSort: "CreatedAt",
            defaultSortDir: SortDirection.Desc,
            ct);

        return new PagedResult<OrderResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = mapper.Map<IReadOnlyList<OrderResponse>>(paged.Items)
        };
    }
}
