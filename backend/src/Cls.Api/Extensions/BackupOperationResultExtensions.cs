using Cls.Application.Backups;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Extensions;

public static class BackupOperationResultExtensions
{
    public static IActionResult ToActionResult<T>(this BackupOperationResult<T> result, Func<T, IActionResult> onSuccess)
    {
        return result.Status switch
        {
            BackupOperationStatus.Success => onSuccess(result.Value!),
            BackupOperationStatus.NotFound => new NotFoundResult(),
            BackupOperationStatus.Conflict => new ConflictObjectResult(new { message = result.Message }),
            BackupOperationStatus.BadRequest => new BadRequestObjectResult(new { message = result.Message }),
            _ => throw new InvalidOperationException($"Unexpected backup operation status: {result.Status}")
        };
    }
}
