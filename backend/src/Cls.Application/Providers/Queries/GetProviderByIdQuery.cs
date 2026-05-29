using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Files;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Providers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Providers.Queries;

public record GetProviderByIdQuery(int Id) : IRequest<ProviderResponse?>;

public class GetProviderByIdQueryHandler(IUnitOfWork uow, IMapper mapper, IMediator mediator)
    : IRequestHandler<GetProviderByIdQuery, ProviderResponse?>
{
    public async Task<ProviderResponse?> Handle(GetProviderByIdQuery request, CancellationToken ct)
    {
        var provider = await uow.Providers.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (provider is null)
            return null;

        var response = mapper.Map<ProviderResponse>(provider);
        if (provider.ProfileFileId.HasValue)
        {
            response.ProfileUrl = await mediator.Send(
                new GetDownloadUrlQuery(provider.ProfileFileId.Value, TimeSpan.FromHours(1)), ct);
        }

        return response;
    }
}
