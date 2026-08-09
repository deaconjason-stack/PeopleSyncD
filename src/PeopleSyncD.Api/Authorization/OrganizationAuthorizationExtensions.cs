using Microsoft.AspNetCore.Authorization;
using PeopleSyncD.Application.Authorization;

namespace PeopleSyncD.Api.Authorization;

public static class OrganizationAuthorizationExtensions
{
    public static RouteHandlerBuilder RequireOrganizationAccess(this RouteHandlerBuilder endpoint, MembershipRoleRequirement requirement)
    {
        return endpoint.RequireAuthorization(new AuthorizeAttribute { Policy = requirement.PolicyName });
    }
}

public sealed record MembershipRoleRequirement(string PolicyName);
