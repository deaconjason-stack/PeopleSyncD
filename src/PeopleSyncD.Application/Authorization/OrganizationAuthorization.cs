using System.Security.Claims;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Authorization;

public static class OrganizationAuthorization
{
    public static bool CanAccessOrganization(ClaimsPrincipal user, Guid organizationId, MembershipRole minimumRole = MembershipRole.Member)
    {
        if (organizationId == Guid.Empty || user.Identity?.IsAuthenticated != true) return false;
        var membership = user.FindFirst("peoplesyncd:membership:" + organizationId);
        if (membership is null) return false;
        return Enum.TryParse<MembershipRole>(membership.Value, true, out var role) && role >= minimumRole;
    }
}
