using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Rbac.Middleware;

public class PermissionAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // No endpoint metadata → allow through
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Collect required permissions
        var requiredPermissions = endpoint
            .Metadata
            .OfType<RequirePermissionAttribute>()
            .SelectMany(attr => attr.Permissions)
            .ToList();

        // No permissions required → allow through
        if (!requiredPermissions.Any())
        {
            await _next(context);
            return;
        }

        // Must be authenticated
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Extract permissions from JWT
        var userPermissions = context.User
            .Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();

        // Check if user has ALL required permissions
        bool hasAll = requiredPermissions.All(rp => userPermissions.Contains(rp));

        if (!hasAll)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        await _next(context);
    }
}
