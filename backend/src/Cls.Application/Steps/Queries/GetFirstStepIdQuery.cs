using Cls.Application.Abstractions;
using Cls.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Steps.Queries;
public record GetFirstStepIdQuery() : IRequest<int>;
public class GetFirstStepIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetFirstStepIdQuery, int>
{
    public async Task<int> Handle(GetFirstStepIdQuery req, CancellationToken ct)
    {
        var step = await uow.Steps.Query().Include(p => p.Stage).OrderBy(p => p.OrderPosition).Take(1).FirstOrDefaultAsync();
        
        if (step is null)
            throw new NotFoundException("Step note found");

        return step.Id;
    }
}
