using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace PGManagement.API.Helpers;

public sealed class FirebaseAuthMiddleware
{
    private static readonly PathString[] AnonymousPaths =
    [
        new PathString("/"),
        new PathString("/api/auth/login"),
        new PathString("/api/auth/admin-login"),
        new PathString("/api/admin"),
        new PathString("/swagger"),
        new PathString("/swagger/index.html"),
        new PathString("/swagger/v1/swagger.json")
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<FirebaseAuthMiddleware> _logger;

    public FirebaseAuthMiddleware(RequestDelegate next, ILogger<FirebaseAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAnonymousPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!TryGetBearerToken(context, out var idToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Missing Authorization bearer token." });
            return;
        }

        FirebaseToken decodedToken;
        try
        {
            decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid Firebase ID token.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid Firebase ID token." });
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
            new Claim(ClaimTypes.Name, decodedToken.Uid)
        };

        foreach (var claim in decodedToken.Claims)
        {
            claims.Add(new Claim($"firebase:{claim.Key}", claim.Value?.ToString() ?? string.Empty));
        }

        var identity = new ClaimsIdentity(claims, "Firebase");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }

    private static bool IsAnonymousPath(PathString path)
    {
        return AnonymousPaths.Any(anonymous => path.StartsWithSegments(anonymous));
    }

    private static bool TryGetBearerToken(HttpContext context, out string token)
    {
        token = string.Empty;
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return false;
        }

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authHeader["Bearer ".Length..].Trim();
        return !string.IsNullOrWhiteSpace(token);
    }
}
