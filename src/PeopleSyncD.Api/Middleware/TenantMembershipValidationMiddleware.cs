using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Api.Middleware;

/// <summary>
/// Rejects tenant tokens whose membership is no longer active or whose role changed after issuance.
/// </summary>
public sealed class TenantMembershipValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IOrganizationMembershipRepository memberships)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.TryGetUserId(out var userId)
            && Guid.TryParse(context.User.FindFirst("tenant_id")?.Value, out var organizationId)
            && Guid.TryParse(context.User.FindFirst("membership_id")?.Value, out var membershipId))
        {
            var membership = await memberships.GetByIdAsync(membershipId, context.RequestAborted);
            var roleClaim = context.User.FindFirst("tenant_role")?.Value;
            var valid = membership is not null
                && membership.UserId == userId
                && membership.OrganizationId == organizationId
                && membership.Status == MembershipStatus.Active
                && string.Equals(membership.Role.ToString(), roleClaim, StringComparison.OrdinalIgnoreCase);
            if (!valid)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new { error = "tenant_session_stale" },
                    context.RequestAborted);
                return;
            }
        }

        await next(context);
    }
}
