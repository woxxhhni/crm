using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Application.Steps.Queries;
using Cls.Shared.Contracts.Steps;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ClsAuthorize]
public class StepsController(IMediator mediator, IMapper mapper) : ClsControllerBase
{
    [HttpGet("get-list")]
    [ClsAuthorize]
    public async Task<ActionResult<IReadOnlyCollection<StepResponse>>> GetList(CancellationToken ct)
    {
        var result = await mediator.Send(new ListStepsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("get-by-stage/{stageId:int}")]
    [ClsAuthorize]
    public async Task<ActionResult<IReadOnlyCollection<StepResponse>>> GetByStageId([FromRoute] int stageId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderStepHistoryByOrderIdQuery(stageId), ct);
        return Ok(mapper.Map<IReadOnlyList<StepResponse>>(result));
    }
}
