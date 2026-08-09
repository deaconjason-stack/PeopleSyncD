using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Authorization;

public sealed class TenantAccess(PeopleSyncDDbContext db)
{
    public async Task<MembershipRole?> GetRoleAsync(ClaimsPrincipal principal, Guid organizationId, CancellationToken cancellationToken)
    {
        if (principal.Identity?.IsAuthenticated != true || organizationId == Guid.Empty) return null;
        var subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId)) return null;
        return await db.OrganizationMemberships
            .Where(x => x.UserId == userId && x.OrganizationId == organizationId)
            .Select(x => (MembershipRole?)x.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static bool MeetsMinimum(MembershipRole? role, MembershipRole minimum) => role.HasValue && role.Value >= minimum;
}
