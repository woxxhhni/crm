using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Files;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Clients.Queries;

public record GetClientByIdQuery(int Id) : IRequest<ClientResponse?>;

public class GetClientByIdQueryHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
    : IRequestHandler<GetClientByIdQuery, ClientResponse?>
{
    public async Task<ClientResponse?> Handle(GetClientByIdQuery request, CancellationToken ct)
    {
        var client = await uow.Clients.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (client is null)
            return null;

        var response = mapper.Map<ClientResponse>(client);
        if (client.ProfileFileId.HasValue)
        {
            response.ProfileUrl = await mediator.Send(
                new GetDownloadUrlQuery(client.ProfileFileId.Value, TimeSpan.FromHours(1)), ct);
        }

        return response;
    }
}
