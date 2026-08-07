using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Application.Identity;

public sealed class SessionTokenService(
    IAccessTokenIssuer accessTokens,
    IRefreshSessionGateway refreshSessions)
{
    public async Task<AccessTokenDto> IssueAsync(
        IdentityUserDto user,
        OrganizationAccessDto? access = null,
        string assuranceLevel = "pwd",
        string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await refreshSessions.IssueAsync(
            user.Id,
            access?.OrganizationId,
            access?.MembershipId,
            assuranceLevel: assuranceLevel,
            deviceLabel: deviceLabel,
            cancellationToken: cancellationToken);
        var accessToken = accessTokens.Issue(
            user,
            access,
            assuranceLevel,
            refreshToken.FamilyId);
        return accessToken with
        {
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
        };
    }
}
