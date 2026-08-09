using PeopleSyncD.Application.Authorization;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Api.Authorization;

public static class OrganizationAuthorizationEndpointFilter
{
    public static async ValueTask<object?> RequireMembership(HttpContext context, Guid organizationId, MembershipRole minimumRole, Func<Task<object?>> next)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        if (!OrganizationAuthorization.CanAccessOrganization(user, organizationId, minimumRole))
            return Results.Forbid();

        return await next();
    }
}
