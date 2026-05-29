using MediatR;
using Cls.Application.Abstractions;
using Cls.Application.Extensions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Cls.Shared.Contracts.Users;

namespace Cls.Application.Users.Queries;
public record ListUsersQuery(string? Role = null, bool? IsActive = null, string? Search = null, PagedRequest? Paging = null) : IRequest<PagedResult<User>>;
public class ListUsersQueryHandler(IUnitOfWork uow) : IRequestHandler<ListUsersQuery, PagedResult<User>>
{
    public async Task<PagedResult<User>> Handle(ListUsersQuery r, CancellationToken ct)
    {
        var q = uow.Users.Query()
            .Where(x=>x.Role != UserRole.Admin).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(r.Role)) q = q.Where(x => x.Role.ToString().ToLower() == r.Role.ToLower());
        if (r.IsActive.HasValue) q = q.Where(x => x.IsActive == r.IsActive);
        if (!string.IsNullOrWhiteSpace(r.Search)) { var s = r.Search.ToLower(); q = q.Where(x => x.Name.ToLower().Contains(s) || x.Email.ToLower().Contains(s)); }
        var paging = r.Paging ?? new PagedRequest();
        return await q.ToPagedResultAsync(paging, defaultSort: "Id", defaultSortDir: SortDirection.Asc, ct);
    }
}
