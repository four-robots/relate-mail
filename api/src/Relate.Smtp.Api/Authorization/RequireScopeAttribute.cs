using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Relate.Smtp.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireScopeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredScopes;

    public RequireScopeAttribute(params string[] scopes)
    {
        _requiredScopes = scopes;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get all scope claims
        var userScopes = user.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .ToList();

        // Check if user has at least one required scope
        var hasRequiredScope = _requiredScopes.Any(requiredScope =>
            userScopes.Contains(requiredScope, StringComparer.OrdinalIgnoreCase));

        if (!hasRequiredScope)
        {
            context.Result = new ForbidResult();
        }
    }
}
