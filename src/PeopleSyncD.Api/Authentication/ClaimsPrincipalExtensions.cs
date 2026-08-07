using System.Globalization;
using System.Security.Claims;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        principal.TryGetUserId(out var userId)
            ? userId
            : throw new InvalidOperationException("The authenticated user identifier is unavailable.");

    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out userId);

    public static bool TryGetSessionFamilyId(this ClaimsPrincipal principal, out Guid familyId) =>
        Guid.TryParse(principal.FindFirstValue("sid"), out familyId);

    public static Guid? GetTenantId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("tenant_id"), out var tenantId) ? tenantId : null;

    public static Guid? GetMembershipId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("membership_id"), out var membershipId) ? membershipId : null;

    public static string GetAssuranceLevel(this ClaimsPrincipal principal) =>
        AuthenticationAssurance.Normalize(principal.FindFirstValue("psd_assurance"));

    public static DateTimeOffset? GetAuthenticationTime(this ClaimsPrincipal principal) =>
        long.TryParse(
            principal.FindFirstValue("auth_time"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;

    public static string GetAuthenticationMethod(this ClaimsPrincipal principal)
    {
        var methods = principal.FindAll("amr").Select(claim => claim.Value).ToArray();
        if (methods.Contains("passkey", StringComparer.Ordinal))
        {
            return "passkey";
        }

        if (methods.Contains("totp", StringComparer.Ordinal))
        {
            return "totp";
        }

        if (methods.Contains("recovery_code", StringComparer.Ordinal))
        {
            return "recovery_code";
        }

        return "pwd";
    }

    public static TenantRole? GetTenantRole(this ClaimsPrincipal principal) =>
        principal.FindFirstValue("tenant_role")?.ToLowerInvariant() switch
        {
            "owner" => TenantRole.Owner,
            "administrator" => TenantRole.Administrator,
            "manager" => TenantRole.Manager,
            "member" => TenantRole.Member,
            "auditor" => TenantRole.Auditor,
            _ => null,
        };

    public static TenantContextDto? GetTenantContext(this ClaimsPrincipal principal)
    {
        var tenantId = principal.GetTenantId();
        var membershipId = principal.GetMembershipId();
        var role = principal.GetTenantRole();
        if (tenantId is null || membershipId is null || role is null)
        {
            return null;
        }

        return new TenantContextDto(
            membershipId.Value,
            tenantId.Value,
            principal.FindFirstValue("tenant_name") ?? string.Empty,
            principal.FindFirstValue("tenant_slug") ?? string.Empty,
            role.Value,
            principal.FindAll("permission").Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray());
    }
}
