using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.General;
using MediatR;

namespace Cls.Application.General.Currencies.Queries;

public record ListCurrenciesQuery() : IRequest<IReadOnlyList<CurrencyResponse>>;

public class ListCurrenciesQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<ListCurrenciesQuery, IReadOnlyList<CurrencyResponse>>
{
    public async Task<IReadOnlyList<CurrencyResponse>> Handle(ListCurrenciesQuery request, CancellationToken ct)
    {
        var currencies = await uow.Currencies.ListAsync(ct: ct);
        return mapper.Map<IReadOnlyList<CurrencyResponse>>(currencies);
    }
}
