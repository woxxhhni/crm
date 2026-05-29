using Cls.Api.Http;
using Cls.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cls.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ClsAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string Roles { get; set; } = string.Empty;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!httpContext.Items.TryGetValue(ClsHttpContextKeys.CurrentUser, out var item) || item is not User dbUser)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!dbUser.IsActive)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (string.IsNullOrWhiteSpace(Roles))
            return;

        var requiredRoles = Roles.Split(',').Select(r => r.Trim().ToLower()).ToList();
        if (!requiredRoles.Contains(dbUser.Role.ToString().ToLower()))
            context.Result = new ForbidResult();
    }
}
