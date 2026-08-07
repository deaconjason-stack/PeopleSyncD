using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Application.Identity;

public sealed class SessionTokenService(
    IAccessTokenIssuer accessTokens,
    IRefreshSessionGateway refreshSessions)
{
    public async Task<AccessTokenDto> IssueAsync(
        IdentityUserDto user,
        OrganizationAccessDto? access = null,
        CancellationToken cancellationToken = default,
        string assuranceLevel = "pwd",
        string? deviceLabel = null)
    {
        var refreshToken = await refreshSessions.IssueAsync(
            user.Id,
            access?.OrganizationId,
            access?.MembershipId,
            cancellationToken: cancellationToken,
            assuranceLevel: assuranceLevel,
            deviceLabel: deviceLabel);
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
