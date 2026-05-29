using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Api.Services;
using Cls.Application.Files;
using Cls.Application.ProviderFileGroups.Commands;
using Cls.Application.ProviderFileGroups.Queries;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Files;
using Cls.Shared.Contracts.Providers.FileGroups;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/providers/{providerId:int}/file-groups")]
[ClsAuthorize]
public class ProviderFileGroupsController(IMediator mediator, IMapper mapper, IFileService fileService) : ClsControllerBase
{
    public sealed class GroupFileRequest
    {
        public List<IFormFile> Files { get; set; } = default!;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProviderFileGroupResponse>>> List(int providerId)
    {
        var list = await mediator.Send(new ListProviderFileGroupsQuery(providerId));
        var result = new List<ProviderFileGroupResponse>();
        foreach (var g in list)
        {
            var r = mapper.Map<ProviderFileGroupResponse>(g);
            r.GroupItems = new List<ProviderFileGroupItemResponse>();
            foreach (var it in g.Items)
            {
                var link = await fileService.GetFileUrl(it.FileId, CancellationToken.None);
                r.GroupItems.Add(new ProviderFileGroupItemResponse() { Id = it.Id, ProviderFileGroupId = it.ProviderFileGroupId, FileId = it.FileId, FileName = it.File.OriginalFilename, Url = link });
            }
            result.Add(r);
        }
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProviderFileGroupResponse>> GetById(int providerId, int id)
    {
        var e = await mediator.Send(new GetProviderFileGroupByIdQuery(id));
        if (e is null || e.ProviderId != providerId) return NotFound();
        ProviderFileGroupResponse resp = mapper.Map<ProviderFileGroupResponse>(e);
        if (e.Items is not null)
        {
            resp.GroupItems = new List<ProviderFileGroupItemResponse>();
            foreach (var it in e.Items)
            {
                var link = await fileService.GetFileUrl(it.FileId, CancellationToken.None);
                resp.GroupItems.Add(new ProviderFileGroupItemResponse() { Id = it.Id, ProviderFileGroupId = it.ProviderFileGroupId, FileId = it.FileId, FileName = it.File.OriginalFilename, Url = link });
            }
        }
        return Ok(resp);
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50Mb
    public async Task<ActionResult<ProviderFileGroupResponse>> Create([FromRoute] int providerId, [FromForm] string label, [FromForm] GroupFileRequest? files, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId)) return Forbid();
        var fileIds = new List<int>();
        foreach (var file in files.Files)
        {
            if (file == null || file.Length == 0) continue;

            await using var stream = file?.OpenReadStream();
            var createdFile = await mediator.Send(new UploadFileCommand(UploadFileBucket.ProviderFileGroup, file.FileName, stream, file.ContentType, file.Length, userId), ct);
            fileIds.Add(createdFile.Id);
        }
        var created = await mediator.Send(new CreateProviderFileGroupCommand(providerId, label, userId, fileIds));
        return CreatedAtAction(nameof(GetById), new { providerId = created.ProviderId, id = created.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, mapper.Map<ProviderFileGroupResponse>(created));
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<ProviderFileGroupResponse>> Update(int providerId, int id, [FromBody] ProviderFileGroupUpdateRequest req)
    {
        var updated = await mediator.Send(new UpdateProviderFileGroupCommand(id, req.Label));
        if (updated is null || updated.ProviderId != providerId) return NotFound();
        return Ok(mapper.Map<ProviderFileGroupResponse>(updated));
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Delete(int providerId, int id)
    {
        var ok = await mediator.Send(new DeleteProviderFileGroupCommand(id));
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/items")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50Mb
    public async Task<ActionResult<int>> AddItems([FromRoute] int providerId, [FromRoute] int id, [FromForm] GroupFileRequest? files, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId)) return Forbid();
        var fileIds = new List<int>();
        foreach (var file in files.Files)
        {
            if (file == null || file.Length == 0) continue;

            await using var stream = file?.OpenReadStream();
            var createdFile = await mediator.Send(new UploadFileCommand(UploadFileBucket.ProviderFileGroup, file.FileName, stream, file.ContentType, file.Length, userId), ct);
            fileIds.Add(createdFile.Id);
        }
        var count = await mediator.Send(new AddProviderFileGroupItemsCommand(id, fileIds));
        return Ok(count);
    }

    [HttpDelete("items/{itemId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteItem(int providerId, int itemId)
    {
        var file = await mediator.Send(new GetProviderFileGroupItemByIdQuery(itemId))
            ?? throw new KeyNotFoundException("Item not found");
        var ok = await mediator.Send(new DeleteFileCommand(file.FileId));
        if (!ok) return NotFound();
        return NoContent();
    }
}
