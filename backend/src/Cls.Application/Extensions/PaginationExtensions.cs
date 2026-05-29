using Cls.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cls.Application.Extensions;

public static class PaginationExtensions
{
    public static IOrderedQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string property, bool ascending)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var prop = Expression.PropertyOrField(param, property);
        var keySelector = Expression.Lambda(prop, param);
        var method = ascending ? "OrderBy" : "OrderByDescending";
        var call = Expression.Call(typeof(Queryable), method, new[] { typeof(T), prop.Type }, query.Expression, Expression.Quote(keySelector));
        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(call);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PagedRequest req, string defaultSort = "Id", SortDirection defaultSortDir = SortDirection.Asc, CancellationToken ct = default)
    {
        var total = await query.LongCountAsync(ct);
        if (!string.IsNullOrWhiteSpace(req.SortBy))
        {
            try { query = query.OrderByDynamic(req.SortBy!, (req.SortDir ?? SortDirection.Asc) == SortDirection.Asc); }
            catch { query = query.OrderByDynamic(defaultSort, defaultSortDir == SortDirection.Asc); }
        }
        else query = query.OrderByDynamic(defaultSort, defaultSortDir == SortDirection.Asc);

        if (req.PageSize <= 0)
        {
            var allItems = await query.ToListAsync(ct);
            return new PagedResult<T>
            {
                Page = 1,
                PageSize = 0,
                Total = total,
                Items = allItems
            };
        }

        var page = Math.Max(1, req.Page);
        var size = Math.Max(1, req.PageSize);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return new PagedResult<T> { Page = page, PageSize = size, Total = total, Items = items };
    }
}
