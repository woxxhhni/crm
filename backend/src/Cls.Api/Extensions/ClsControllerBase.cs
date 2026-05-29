using Cls.Shared.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Extensions;

public class ClsControllerBase : ControllerBase
{
    protected static bool TryGetUserId(System.Security.Claims.ClaimsPrincipal user, out int userId) =>
        int.TryParse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out userId);

    protected static PagedRequest BuildPagedRequest(
        int page,
        int pageSize,
        string? sortBy,
        string? sortDir)
    {
        return new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDir = Enum.TryParse<SortDirection>(sortDir, true, out var direction) ? direction : null
        };
    }
}
