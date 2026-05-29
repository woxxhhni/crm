using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Steps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Steps.Queries;

public record ListStepsQuery() : IRequest<IReadOnlyList<StepResponse>>;

public class ListStepsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<ListStepsQuery, IReadOnlyList<StepResponse>>
{
    public async Task<IReadOnlyList<StepResponse>> Handle(ListStepsQuery request, CancellationToken ct)
    {
        var steps = await uow.Steps.Query()
            .Include(x => x.Stage)
            .OrderBy(p => p.OrderPosition)
            .AsNoTracking()
            .ToListAsync(ct);

        return mapper.Map<IReadOnlyList<StepResponse>>(steps);
    }
}
