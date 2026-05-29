using Cls.Api.Http;
using Cls.Application.Abstractions;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Exceptions;
using System.Security.Claims;

namespace Cls.Api.Middlewares;

public class UserCheckerMiddleware(RequestDelegate next)
{
    private static readonly StringComparison PathComparison = StringComparison.OrdinalIgnoreCase;

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is null || IsPublicPath(path))
        {
            await next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException();

        var userId = GetUserId(context);
        var uow = context.RequestServices.GetRequiredService<IUnitOfWork>();
        User dbUser;
        try
        {
            dbUser = await uow.Users.GetByIdAsync(userId, context.RequestAborted);
        }
        catch (Exception)
        {
            throw new UnauthorizedException();
        }

        if (!dbUser.IsActive)
            throw new UnauthorizedException();

        context.Items[ClsHttpContextKeys.CurrentUser] = dbUser;

        var currentUserService = context.RequestServices.GetRequiredService<ICurrentUserService>();
        currentUserService.UserId = dbUser.Id;
        currentUserService.Role = dbUser.Role;

        await next(context);
    }

    private static bool IsPublicPath(string path)
    {
        return path.Contains("/auth", PathComparison) ||
               path.Contains("/swagger", PathComparison);
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim is null || !int.TryParse(idClaim.Value, out var id))
            throw new UnauthorizedException();

        return id;
    }
}
