using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Permissions;
using PeopleSyncD.Infrastructure.Configuration;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class JwtAccessTokenIssuer(JwtOptions options, IClock clock) : IAccessTokenIssuer
{
    public AccessTokenDto Issue(
        IdentityUserDto user,
        OrganizationAccessDto? access = null,
        string assuranceLevel = "pwd",
        Guid? sessionFamilyId = null,
        DateTimeOffset? authenticatedAt = null,
        string? authenticationMethod = null)
    {
        var assurance = AuthenticationAssurance.Normalize(assuranceLevel);
        var now = clock.UtcNow;
        var authenticationTime = authenticatedAt ?? now;
        var method = string.IsNullOrWhiteSpace(authenticationMethod)
            ? AuthenticationAssurance.DefaultMethod(assurance)
            : authenticationMethod.Trim().ToLowerInvariant();
        var expiresAt = now.AddMinutes(options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString("D")),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new("email_verified", user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean),
            new("account_active", user.IsActive ? "true" : "false", ClaimValueTypes.Boolean),
            new("mfa_enrolled", user.MfaEnabled ? "true" : "false", ClaimValueTypes.Boolean),
            new("psd_assurance", assurance),
            new("auth_time", authenticationTime.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
        };
        AddAuthenticationMethodClaims(claims, assurance, method);

        if (sessionFamilyId is not null)
        {
            claims.Add(new Claim("sid", sessionFamilyId.Value.ToString("D")));
        }

        TenantContextDto? tenant = null;
        if (access is not null)
        {
            var permissions = PermissionCatalog.ForRole(access.Role);
            claims.Add(new Claim("membership_id", access.MembershipId.ToString("D")));
            claims.Add(new Claim("tenant_id", access.OrganizationId.ToString("D")));
            claims.Add(new Claim("tenant_name", access.OrganizationName));
            claims.Add(new Claim("tenant_slug", access.OrganizationSlug));
            claims.Add(new Claim("tenant_role", access.Role.ToString().ToLowerInvariant()));
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
            tenant = new TenantContextDto(
                access.MembershipId,
                access.OrganizationId,
                access.OrganizationName,
                access.OrganizationSlug,
                access.Role,
                Array.AsReadOnly(permissions.ToArray()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                SecurityAlgorithms.HmacSha256),
        };
        var handler = new JsonWebTokenHandler();
        return new AccessTokenDto(
            handler.CreateToken(descriptor),
            "Bearer",
            expiresAt,
            user,
            tenant,
            AssuranceLevel: assurance,
            SessionFamilyId: sessionFamilyId);
    }

    private static void AddAuthenticationMethodClaims(
        List<Claim> claims,
        string assurance,
        string method)
    {
        if (assurance == AuthenticationAssurance.PhishingResistant)
        {
            claims.Add(new Claim("amr", "passkey"));
            return;
        }

        claims.Add(new Claim("amr", "pwd"));
        if (assurance == AuthenticationAssurance.Mfa && method != "pwd")
        {
            claims.Add(new Claim("amr", method));
        }
    }
}
