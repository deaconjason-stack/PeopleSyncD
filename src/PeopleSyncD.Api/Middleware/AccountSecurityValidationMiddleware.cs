using Microsoft.AspNetCore.Authorization;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Api.Middleware;

/// <summary>
/// Enforces live account-security and session revocation state for authenticated requests.
/// </summary>
public sealed class AccountSecurityValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IIdentityGateway identities,
        IRefreshSessionGateway sessions)
    {
        if (context.User.Identity?.IsAuthenticated != true
            || context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        if (!context.User.TryGetUserId(out var userId)
            || !context.User.TryGetSessionFamilyId(out var familyId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var user = await identities.GetByIdAsync(userId, context.RequestAborted);
        if (user is null || !user.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!await sessions.IsFamilyActiveAsync(userId, familyId, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (user.MfaEnabled
            && !string.Equals(context.User.GetAssuranceLevel(), "mfa", StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
