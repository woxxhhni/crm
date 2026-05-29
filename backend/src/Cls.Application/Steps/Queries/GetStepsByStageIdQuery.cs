using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Steps.Queries;

public record GetStepsByStageIdQuery(int StageId) : IRequest<IReadOnlyList<Step>>;
public class GetStepsByStageIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetStepsByStageIdQuery, IReadOnlyList<Step>>
{
    public async Task<IReadOnlyList<Step>> Handle(GetStepsByStageIdQuery req, CancellationToken ct)
        => await uow.Steps.Query().Include(p => p.Stage).Where(p => p.StageId == req.StageId).ToListAsync();
}
