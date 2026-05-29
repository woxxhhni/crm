using Asp.Versioning;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Api.Services;
using Cls.Application.Providers.Commands;
using Cls.Application.Providers.Queries;
using Cls.Shared.Contracts.Common;
using Cls.Shared.Contracts.Providers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ClsAuthorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProvidersController(
    IMediator mediator,
    IProfilePictureOrchestrator profilePictures) : ClsControllerBase
{
    public sealed class ProfileFileRequest
    {
        public IFormFile? File { get; set; } = default!;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProviderResponse>>> List(
        [FromQuery] string? name,
        [FromQuery] bool? isActive,
        [FromQuery] string? email,
        [FromQuery] string? website,
        [FromQuery] string? phone,
        [FromQuery] string? address,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListProvidersQuery(
            name,
            isActive,
            email,
            website,
            phone,
            address,
            BuildPagedRequest(page, pageSize, sortBy, sortDir)), ct);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProviderResponse>> Get(int id, CancellationToken ct)
    {
        var response = await mediator.Send(new GetProviderByIdQuery(id), ct);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_242_880)]
    public async Task<ActionResult<ProviderResponse>> Create(
        [FromForm] ProviderCreateRequest req,
        [FromForm] ProfileFileRequest profile,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var created = await mediator.Send(new CreateProviderCommand(
            req.Name,
            req.Phone,
            req.SecondPhone,
            req.Email,
            req.Website,
            req.Address,
            req.Description,
            req.IsActive), ct);

        await profilePictures.ApplyProviderProfileAsync(created.Id, userId, profile.File, ct);

        var response = await mediator.Send(new GetProviderByIdQuery(created.Id), ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_242_880)]
    public async Task<ActionResult<ProviderResponse>> Update(
        int id,
        [FromForm] ProviderUpdateRequest req,
        [FromForm] ProfileFileRequest profile,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
            return Forbid();

        var updated = await mediator.Send(new UpdateProviderCommand(
            id,
            req.Name,
            req.Phone,
            req.SecondPhone,
            req.Email,
            req.Website,
            req.Address,
            req.Description,
            req.IsActive), ct);

        if (updated is null)
            return NotFound();

        await profilePictures.ApplyProviderProfileAsync(updated.Id, userId, profile.File, ct);

        var response = await mediator.Send(new GetProviderByIdQuery(updated.Id), ct);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new DeleteProviderCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}
