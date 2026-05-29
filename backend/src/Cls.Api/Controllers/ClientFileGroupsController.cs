using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Application.ClientFileGroups.Commands;
using Cls.Application.ClientFileGroups.Queries;
using Cls.Application.Files;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Clients.FileGroups;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clients/{clientId:int}/file-groups")]
[ClsAuthorize]
public class ClientFileGroupsController(IMediator mediator, IMapper mapper) : ClsControllerBase
{
    public sealed class GroupFileRequest
    {
        public List<IFormFile> Files { get; set; } = default!;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientFileGroupResponse>>> List(int clientId)
    {
        var list = await mediator.Send(new ListClientFileGroupsQuery(clientId));
        var result = new List<ClientFileGroupResponse>();
        foreach (var g in list)
        {
            var r = mapper.Map<ClientFileGroupResponse>(g);
            r.GroupItems = new List<ClientFileGroupItemResponse>();
            foreach (var it in g.Items)
            {
                var link = await mediator.Send(new GetDownloadUrlQuery(it.FileId, TimeSpan.FromHours(1)));
                r.GroupItems.Add(new ClientFileGroupItemResponse() { Id = it.Id, ClientFileGroupId = it.ClientFileGroupId, FileId = it.FileId, FileName = it.File.OriginalFilename, Url = link });
            }
            result.Add(r);
        }
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientFileGroupResponse>> GetById(int clientId, int id)
    {
        var e = await mediator.Send(new GetClientFileGroupByIdQuery(id));
        if (e is null || e.ClientId != clientId) return NotFound();
        ClientFileGroupResponse resp = mapper.Map<ClientFileGroupResponse>(e);
        if (e.Items is not null)
        {
            resp.GroupItems = new List<ClientFileGroupItemResponse>();
            foreach (var it in e.Items)
            {
                var link = await mediator.Send(new GetDownloadUrlQuery(it.FileId, TimeSpan.FromHours(1)));
                resp.GroupItems.Add(new ClientFileGroupItemResponse() { Id = it.Id, ClientFileGroupId = it.ClientFileGroupId, FileId = it.FileId, FileName = it.File.OriginalFilename, Url = link });
            }
        }
        return Ok(resp);
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50Mb
    public async Task<ActionResult<ClientFileGroupResponse>> Create([FromRoute] int clientId, [FromForm] string label, [FromForm] GroupFileRequest? files, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId)) return Forbid();
        var fileIds = new List<int>();
        foreach (var file in files.Files)
        {
            if (file == null || file.Length == 0) continue;

            await using var stream = file?.OpenReadStream();
            var createdFile = await mediator.Send(new UploadFileCommand(UploadFileBucket.ClientFileGroup, file.FileName, stream, file.ContentType, file.Length, userId), ct);
            fileIds.Add(createdFile.Id);
        }
        var created = await mediator.Send(new CreateClientFileGroupCommand(clientId, label, userId, fileIds));
        return CreatedAtAction(nameof(GetById), new { clientId = created.ClientId, id = created.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, mapper.Map<ClientFileGroupResponse>(created));
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    public async Task<ActionResult<ClientFileGroupResponse>> Update(int clientId, int id, [FromBody] ClientFileGroupUpdateRequest req)
    {
        var updated = await mediator.Send(new UpdateClientFileGroupCommand(id, req.Label));
        if (updated is null || updated.ClientId != clientId) return NotFound();
        return Ok(mapper.Map<ClientFileGroupResponse>(updated));
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Delete(int clientId, int id)
    {
        var ok = await mediator.Send(new DeleteClientFileGroupCommand(id));
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/items")]
    [ClsAuthorize(Roles = "Admin,Manager,Employee")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50Mb
    public async Task<ActionResult<int>> AddItems([FromRoute] int clientId, [FromRoute] int id, [FromForm] GroupFileRequest? files, CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId)) return Forbid();
        var fileIds = new List<int>();
        foreach (var file in files.Files)
        {
            if (file == null || file.Length == 0) continue;

            await using var stream = file?.OpenReadStream();
            var createdFile = await mediator.Send(new UploadFileCommand(UploadFileBucket.ClientFileGroup, file.FileName, stream, file.ContentType, file.Length, userId), ct);
            fileIds.Add(createdFile.Id);
        }
        var count = await mediator.Send(new AddClientFileGroupItemsCommand(id, fileIds));
        return Ok(count);
    }

    [HttpDelete("items/{itemId:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteItem(int clientId, int itemId)
    {
        var file = await mediator.Send(new GetClientFileGroupItemByIdQuery(itemId))
            ?? throw new KeyNotFoundException("Item not found");
        var ok = await mediator.Send(new DeleteFileCommand(file.FileId));
        if (!ok) return NotFound();
        return NoContent();
    }
}
