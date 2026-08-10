using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class SessionTokenService(
    IAccessTokenIssuer accessTokens,
    IRefreshSessionGateway refreshSessions,
    IClock clock)
{
    public async Task<AccessTokenDto> IssueAsync(
        IdentityUserDto user,
        OrganizationAccessDto? access = null,
        string assuranceLevel = "pwd",
        string? deviceLabel = null,
        DateTimeOffset? authenticatedAt = null,
        string? authenticationMethod = null,
        CancellationToken cancellationToken = default)
    {
        var assurance = AuthenticationAssurance.Normalize(assuranceLevel);
        var authenticationTime = authenticatedAt ?? clock.UtcNow;
        var method = string.IsNullOrWhiteSpace(authenticationMethod)
            ? AuthenticationAssurance.DefaultMethod(assurance)
            : authenticationMethod.Trim().ToLowerInvariant();
        var refreshToken = await refreshSessions.IssueAsync(
            user.Id,
            access?.OrganizationId,
            access?.MembershipId,
            assuranceLevel: assurance,
            deviceLabel: deviceLabel,
            authenticatedAt: authenticationTime,
            authenticationMethod: method,
            cancellationToken: cancellationToken);
        var accessToken = accessTokens.Issue(
            user,
            access,
            assurance,
            refreshToken.FamilyId,
            authenticationTime,
            method);
        return accessToken with
        {
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
        };
    }
}
