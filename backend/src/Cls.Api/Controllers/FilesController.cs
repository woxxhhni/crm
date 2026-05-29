using System.Threading;
using Microsoft.AspNetCore.Http;
﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cls.Application.Files;
using Cls.Application.Abstractions;
using Cls.Domain.Enums;
using Asp.Versioning;
using Cls.Api.Attributes;
using Cls.Api.Extensions;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FilesController(IMediator mediator, IObjectStorageService storage) : ClsControllerBase
{
    public sealed class UploadFileRequest
    {
        public IFormFile File { get; set; } = default!;
    }

    [HttpPost("upload")]
    [ClsAuthorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_242_880)] // 5Mb
    public async Task<IActionResult> Upload([FromForm] UploadFileRequest req, [FromForm] int uploadedByUserId, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
            return UnprocessableEntity("File required.");

        await using var stream = req.File.OpenReadStream();
        var created = await mediator.Send(
            new UploadFileCommand(UploadFileBucket.GeneralBucket, req.File.FileName, stream, req.File.ContentType, req.File.Length, uploadedByUserId), ct);
        return Ok(created);
    }

    [HttpGet("{id:int}/download-url")]
    [ClsAuthorize]
    public async Task<IActionResult> DownloadUrl(int id, [FromQuery] int minutes = 10)
    {
        var url = await mediator.Send(new GetDownloadUrlQuery(id, TimeSpan.FromMinutes(Math.Clamp(minutes,1,60))));
        return Ok(new { url });
    }

    [HttpGet("{id:int}/content")]
    [ClsAuthorize]
    public async Task<IActionResult> Content(int id, CancellationToken ct)
    {
        var file = await mediator.Send(new GetFileByIdQuery(id), ct);
        if (file is null) return NotFound();

        var objectName = !string.IsNullOrWhiteSpace(file.FilePath) ? file.FilePath : file.StoredFilename;
        var stream = new MemoryStream();
        await storage.DownloadAsync(objectName, stream, ct);
        stream.Position = 0;

        return File(stream, file.MimeType ?? "application/octet-stream", enableRangeProcessing: true);
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await mediator.Send(new DeleteFileCommand(id));
        return ok ? NoContent() : NotFound();
    }
}
