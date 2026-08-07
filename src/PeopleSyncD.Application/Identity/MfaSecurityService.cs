using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Coordinates TOTP enrollment, login challenge completion, step-up authentication, and recovery factors.
/// </summary>
public sealed class MfaSecurityService(
    IMfaSecurityGateway mfa,
    IIdentityGateway identities,
    IOrganizationMembershipRepository memberships,
    SessionTokenService sessions)
{
    public Task<Result<MfaTotpEnrollmentDto>> BeginTotpEnrollmentAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        mfa.BeginTotpEnrollmentAsync(userId, cancellationToken);

    public Task<Result<RecoveryCodeBatchDto>> ConfirmTotpEnrollmentAsync(
        Guid userId,
        ConfirmTotpEnrollmentRequest request,
        CancellationToken cancellationToken = default) =>
        mfa.ConfirmTotpEnrollmentAsync(userId, request.Code, cancellationToken);

    public Task<Result<RecoveryCodeBatchDto>> RegenerateRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        mfa.RegenerateRecoveryCodesAsync(userId, cancellationToken);

    public Task<Result<MfaChallengeDto>> StartStepUpAsync(
        Guid userId,
        TenantContextDto? tenant,
        CancellationToken cancellationToken = default) =>
        mfa.CreateChallengeAsync(
            userId,
            "step_up",
            tenant?.OrganizationId,
            tenant?.MembershipId,
            cancellationToken);

    public async Task<Result<AccessTokenDto>> CompleteChallengeAsync(
        MfaChallengeRequest request,
        CancellationToken cancellationToken = default,
        string? deviceLabel = null)
    {
        var completed = await mfa.CompleteChallengeAsync(request, cancellationToken);
        if (completed.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(completed.Error);
        }

        var user = await identities.GetByIdAsync(completed.Value.UserId, cancellationToken);
        if (user is null || !user.IsActive || !user.MfaEnabled)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.user_unavailable",
                "The account cannot complete multi-factor authentication."));
        }

        OrganizationAccessDto? access = null;
        if (completed.Value.OrganizationId is not null || completed.Value.MembershipId is not null)
        {
            if (completed.Value.OrganizationId is null || completed.Value.MembershipId is null)
            {
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "authentication.challenge_context_invalid",
                    "The multi-factor challenge context is invalid."));
            }

            var membership = await memberships.GetByIdAsync(completed.Value.MembershipId.Value, cancellationToken);
            if (membership is null
                || membership.UserId != user.Id
                || membership.OrganizationId != completed.Value.OrganizationId.Value
                || membership.Status != MembershipStatus.Active)
            {
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "authentication.challenge_membership_invalid",
                    "The organization membership is no longer active."));
            }

            var available = await memberships.ListForUserAsync(user.Id, cancellationToken);
            access = available.SingleOrDefault(item => item.MembershipId == membership.Id);
            if (access is null)
            {
                return Result.Failure<AccessTokenDto>(new DomainError(
                    "authentication.challenge_membership_unavailable",
                    "The organization membership context is unavailable."));
            }
        }

        return Result.Success(await sessions.IssueAsync(
            user,
            access,
            cancellationToken,
            assuranceLevel: "mfa",
            deviceLabel: deviceLabel));
    }

    public Task<int> GetRecoveryCodeCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        mfa.GetRecoveryCodeCountAsync(userId, cancellationToken);

    public Task<IReadOnlyCollection<SecurityEventDto>> ListSecurityEventsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        mfa.ListSecurityEventsAsync(userId, cancellationToken: cancellationToken);
}
