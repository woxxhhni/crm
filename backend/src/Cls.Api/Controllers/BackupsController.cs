using Asp.Versioning;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Application.Backups.Commands;
using Cls.Application.Backups.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ClsAuthorize(Roles = "Admin")]
public class BackupsController(IMediator mediator) : ClsControllerBase
{
    /// <summary>List all backup jobs (history), newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var jobs = await mediator.Send(new ListBackupJobsQuery(), ct);
        return Ok(jobs);
    }

    /// <summary>Get a single backup job by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var job = await mediator.Send(new GetBackupJobByIdQuery(id), ct);
        return job is null ? NotFound() : Ok(job);
    }

    /// <summary>Request a new backup export (async — returns immediately).</summary>
    [HttpPost]
    public async Task<IActionResult> CreateBackup(CancellationToken ct)
    {
        var result = await mediator.Send(new CreateBackupExportCommand(), ct);
        return result.ToActionResult(Ok);
    }

    /// <summary>Get download URL for a completed backup.</summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBackupDownloadUrlQuery(id), ct);
        return result.ToActionResult(Ok);
    }

    /// <summary>Restore from an existing backup in the system.</summary>
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> RestoreFromExisting(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new RestoreFromBackupCommand(id), ct);
        return result.ToActionResult(Ok);
    }

    /// <summary>Upload a ZIP file and start a restore from it.</summary>
    [HttpPost("upload-restore")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)] // 500 MB
    public async Task<IActionResult> UploadAndRestore(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "A ZIP file is required." });

        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadAndRestoreBackupCommand(stream, file.FileName, file.Length),
            ct);

        return result.ToActionResult(Ok);
    }

    /// <summary>Delete a backup job and its ZIP file from MinIO.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBackupJobCommand(id), ct);
        return result.ToActionResult(_ => NoContent());
    }
}
