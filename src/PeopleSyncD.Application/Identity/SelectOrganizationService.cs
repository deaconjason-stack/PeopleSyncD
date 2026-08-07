using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Exchanges an authenticated user context for an active tenant context.
/// </summary>
public sealed class SelectOrganizationService(
    IIdentityGateway identities,
    IOrganizationMembershipRepository memberships,
    SessionTokenService sessions)
{
    public async Task<Result<AccessTokenDto>> ExecuteAsync(
        Guid userId,
        SelectOrganizationRequest request,
        CancellationToken cancellationToken = default,
        string assuranceLevel = "pwd",
        string? deviceLabel = null)
    {
        if (userId == Guid.Empty || request.OrganizationId == Guid.Empty)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "tenant.context_invalid",
                "A valid user and organization are required."));
        }

        var user = await identities.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.user_unavailable",
                "The authenticated user is unavailable."));
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.email_verification_required",
                "Email verification is required before tenant access can be issued."));
        }

        if (user.MfaEnabled && !string.Equals(assuranceLevel, "mfa", StringComparison.Ordinal))
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.mfa_required",
                "A verified second factor is required before tenant access can be issued."));
        }

        var membership = await memberships.GetActiveAsync(userId, request.OrganizationId, cancellationToken);
        if (membership is null || membership.Status != MembershipStatus.Active)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "tenant.access_denied",
                "The user does not have an active membership in this organization."));
        }

        var available = await memberships.ListForUserAsync(userId, cancellationToken);
        var access = available.SingleOrDefault(item => item.MembershipId == membership.Id);
        return access is null
            ? Result.Failure<AccessTokenDto>(new DomainError(
                "tenant.access_unavailable",
                "The tenant access projection is unavailable."))
            : Result.Success(await sessions.IssueAsync(
                user,
                access,
                cancellationToken,
                assuranceLevel,
                deviceLabel));
    }
}
