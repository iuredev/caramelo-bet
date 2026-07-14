using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CarameloBet.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarameloBet.API.Middlewares;

public class ActiveUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUserRepository userRepository)
    {
        var endpoint = context.GetEndpoint();
        var metadata = endpoint?.Metadata;
        var requiresAuthorization = metadata?.GetMetadata<IAuthorizeData>() is not null;
        var allowsAnonymous = metadata?.GetMetadata<IAllowAnonymous>() is not null;

        if (!requiresAuthorization || allowsAnonymous || context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Invalid user");
            return;
        }

        var user = await userRepository.GetByIdAsync(userId);

        if (user is null || user.Status == "deleted")
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Invalid user");
            return;
        }

        if (user.ReleaseExpiredBlock(DateTime.UtcNow))
        {
            await userRepository.SaveChangesAsync();
        }

        if (user.Status == "blocked")
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "User is blocked");
            return;
        }

        await next(context);
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string detail)
    {
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status403Forbidden
                ? "Forbidden"
                : "Unauthorized",
            Detail = detail
        });
    }
}
