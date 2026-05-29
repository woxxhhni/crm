using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Extensions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Clients;
using Cls.Shared.Contracts.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Clients.Queries;

public record ListClientsQuery(
    string? Name = null,
    bool? IsActive = null,
    string? Email = null,
    string? Website = null,
    string? Phone = null,
    string? Address = null,
    PagedRequest? Paging = null) : IRequest<PagedResult<ClientResponse>>;

public class ListClientsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<ListClientsQuery, PagedResult<ClientResponse>>
{
    public async Task<PagedResult<ClientResponse>> Handle(ListClientsQuery request, CancellationToken ct)
    {
        var q = uow.Clients.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.ToLower();
            q = q.Where(x => x.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.ToLower();
            q = q.Where(x => x.Email != null && x.Email.ToLower().Contains(email));
        }

        if (request.IsActive.HasValue)
            q = q.Where(x => x.IsActive == request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            var website = request.Website.ToLower();
            q = q.Where(x => x.Website.ToLower().Contains(website));
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = request.Phone.ToLower();
            q = q.Where(x => x.Phone.ToLower().Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var address = request.Address.ToLower();
            q = q.Where(x => x.Address.ToLower().Contains(address));
        }

        var paged = await q.ToPagedResultAsync(
            request.Paging ?? new PagedRequest(),
            defaultSort: "Id",
            defaultSortDir: SortDirection.Asc,
            ct);

        return new PagedResult<ClientResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = mapper.Map<IReadOnlyList<ClientResponse>>(paged.Items)
        };
    }
}
