using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class RefreshSessionService(
    IRefreshSessionGateway refreshSessions,
    IIdentityGateway identities,
    IOrganizationMembershipRepository memberships,
    IAccessTokenIssuer accessTokens)
{
    public async Task<Result<AccessTokenDto>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "refresh.invalid",
                "A refresh token is required."));
        }

        var rotation = await refreshSessions.RotateAsync(request.RefreshToken, cancellationToken);
        if (rotation.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(rotation.Error);
        }

        var grant = rotation.Value;
        var user = await identities.GetByIdAsync(grant.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await refreshSessions.RevokeFamilyAsync(grant.FamilyId, "account_security_changed", cancellationToken);
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.user_unavailable",
                "Reauthentication is required."));
        }

        if (user.MfaEnabled && !string.Equals(grant.AssuranceLevel, "mfa", StringComparison.Ordinal))
        {
            await refreshSessions.RevokeFamilyAsync(grant.FamilyId, "mfa_assurance_required", cancellationToken);
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.mfa_required",
                "Multi-factor reauthentication is required."));
        }

        OrganizationAccessDto? access = null;
        if (grant.OrganizationId is not null || grant.MembershipId is not null)
        {
            if (grant.OrganizationId is null || grant.MembershipId is null || !user.EmailConfirmed)
            {
                await refreshSessions.RevokeFamilyAsync(grant.FamilyId, "tenant_context_invalid", cancellationToken);
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "refresh.tenant_invalid",
                    "The tenant refresh context is no longer valid."));
            }

            var membership = await memberships.GetByIdAsync(grant.MembershipId.Value, cancellationToken);
            if (membership is null
                || membership.UserId != user.Id
                || membership.OrganizationId != grant.OrganizationId.Value
                || membership.Status != MembershipStatus.Active)
            {
                await refreshSessions.RevokeFamilyAsync(grant.FamilyId, "membership_inactive", cancellationToken);
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "refresh.membership_inactive",
                    "The membership no longer permits session refresh."));
            }

            var available = await memberships.ListForUserAsync(user.Id, cancellationToken);
            access = available.SingleOrDefault(item => item.MembershipId == membership.Id);
            if (access is null)
            {
                await refreshSessions.RevokeFamilyAsync(grant.FamilyId, "membership_projection_missing", cancellationToken);
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "refresh.membership_unavailable",
                    "The membership context is unavailable."));
            }
        }

        var accessToken = accessTokens.Issue(user, access, grant.AssuranceLevel, grant.FamilyId);
        return Result.Success(accessToken with
        {
            RefreshToken = grant.Replacement.Token,
            RefreshTokenExpiresAt = grant.Replacement.ExpiresAt,
        });
    }
}
