using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Api.Authentication;

/// <summary>
/// Safe extraction helpers for validated PeopleSyncD access-token claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out userId);

    public static bool TryGetTenantId(this ClaimsPrincipal principal, out Guid tenantId) =>
        Guid.TryParse(principal.FindFirst("tenant_id")?.Value, out tenantId);

    public static bool TryGetSessionFamilyId(this ClaimsPrincipal principal, out Guid familyId) =>
        Guid.TryParse(principal.FindFirst("sid")?.Value, out familyId);

    public static string GetAssuranceLevel(this ClaimsPrincipal principal) =>
        string.Equals(principal.FindFirst("psd_assurance")?.Value, "mfa", StringComparison.Ordinal)
            ? "mfa"
            : "pwd";

    public static TenantContextDto? GetTenantContext(this ClaimsPrincipal principal)
    {
        if (!Guid.TryParse(principal.FindFirst("membership_id")?.Value, out var membershipId)
            || !Guid.TryParse(principal.FindFirst("tenant_id")?.Value, out var organizationId)
            || !Enum.TryParse<TenantRole>(
                principal.FindFirst("tenant_role")?.Value,
                true,
                out var role))
        {
            return null;
        }

        var name = principal.FindFirst("tenant_name")?.Value;
        var slug = principal.FindFirst("tenant_slug")?.Value;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var permissions = principal.FindAll("permission")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return new TenantContextDto(
            membershipId,
            organizationId,
            name,
            slug,
            role,
            Array.AsReadOnly(permissions));
    }
}
