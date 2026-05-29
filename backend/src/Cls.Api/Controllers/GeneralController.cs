using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Api.Services;
using Cls.Application.General.Currencies.Queries;
using Cls.Application.Users.Queries;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.General;
using Cls.Shared.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cls.Shared.Contracts.Abstractions;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GeneralController(IMediator mediator, ICurrentUserService currentUserService, IMapper mapper, IFileService fileService) : ClsControllerBase
{
    [HttpGet("CurrencyList")]
    [ClsAuthorize]
    public async Task<ActionResult<IReadOnlyList<CurrencyResponse>>> GetCurrencyList(CancellationToken ct)
    {
        var currs = await mediator.Send(new ListCurrenciesQuery(), ct);

        return Ok(currs);
    }

    [HttpGet("OrderStatusList")]
    [ClsAuthorize]
    public ActionResult<Dictionary<int, string>> GetOrderStatusList(CancellationToken ct)
    {
        var sts = Enum.GetValues(typeof(OrderStatus))
                      .Cast<OrderStatus>()
                      .ToDictionary(t => (int)t, t => t.ToString());

        return Ok(sts);
    }

    [HttpGet("me")]
    [ClsAuthorize]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserByIdQuery(currentUserService.UserId), ct);
        if (user == null)
            return NotFound();
        var me = mapper.Map<MeResponse>(user);
        if (me.ProfileFileId != null)
            me.ProfileUrl = await fileService.GetFileUrl(me.ProfileFileId.Value, ct);

        return Ok(me);
    }
}
