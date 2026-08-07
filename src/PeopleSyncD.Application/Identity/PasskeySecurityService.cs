using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class PasskeySecurityService(
    IPasskeySecurityGateway passkeys,
    IIdentityGateway identities,
    IOrganizationMembershipRepository memberships,
    SessionTokenService sessions)
{
    public Task<Result<PasskeyCeremonyOptionsDto>> BeginRegistrationAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        passkeys.BeginRegistrationAsync(userId, cancellationToken);

    public Task<Result<PasskeyCredentialDto>> CompleteRegistrationAsync(
        Guid userId,
        CompletePasskeyRegistrationRequest request,
        CancellationToken cancellationToken = default) =>
        passkeys.CompleteRegistrationAsync(userId, request, cancellationToken);

    public async Task<Result<PasskeyCeremonyOptionsDto>> BeginLoginAsync(
        BeginPasskeyAuthenticationRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await identities.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return PasskeyUnavailable<PasskeyCeremonyOptionsDto>();
        }

        return await passkeys.BeginAuthenticationAsync(user.Id, "login", cancellationToken: cancellationToken);
    }

    public async Task<Result<AccessTokenDto>> CompleteLoginAsync(
        CompletePasskeyAuthenticationRequest request,
        string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var completed = await passkeys.CompleteAuthenticationAsync(request, cancellationToken);
        if (completed.IsFailure || completed.Value.Purpose != "login")
        {
            return PasskeyUnavailable<AccessTokenDto>();
        }

        var user = await identities.GetByIdAsync(completed.Value.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return PasskeyUnavailable<AccessTokenDto>();
        }

        return Result.Success(await sessions.IssueAsync(
            user,
            assuranceLevel: AuthenticationAssurance.PhishingResistant,
            deviceLabel: deviceLabel,
            authenticationMethod: "passkey",
            cancellationToken: cancellationToken));
    }

    public Task<Result<PasskeyCeremonyOptionsDto>> BeginStepUpAsync(
        Guid userId,
        TenantContextDto? tenant,
        CancellationToken cancellationToken = default) =>
        passkeys.BeginAuthenticationAsync(
            userId,
            "step_up",
            tenant?.OrganizationId,
            tenant?.MembershipId,
            cancellationToken);

    public async Task<Result<AccessTokenDto>> CompleteStepUpAsync(
        Guid expectedUserId,
        CompletePasskeyAuthenticationRequest request,
        string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var completed = await passkeys.CompleteAuthenticationAsync(request, cancellationToken);
        if (completed.IsFailure
            || completed.Value.Purpose != "step_up"
            || completed.Value.UserId != expectedUserId)
        {
            return PasskeyUnavailable<AccessTokenDto>();
        }

        var user = await identities.GetByIdAsync(completed.Value.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return PasskeyUnavailable<AccessTokenDto>();
        }

        OrganizationAccessDto? access = null;
        if (completed.Value.OrganizationId is not null || completed.Value.MembershipId is not null)
        {
            if (completed.Value.OrganizationId is null || completed.Value.MembershipId is null)
            {
                return PasskeyUnavailable<AccessTokenDto>();
            }

            var membership = await memberships.GetByIdAsync(completed.Value.MembershipId.Value, cancellationToken);
            if (membership is null
                || membership.UserId != user.Id
                || membership.OrganizationId != completed.Value.OrganizationId.Value
                || membership.Status != MembershipStatus.Active)
            {
                return PasskeyUnavailable<AccessTokenDto>();
            }

            var available = await memberships.ListForUserAsync(user.Id, cancellationToken);
            access = available.SingleOrDefault(item => item.MembershipId == membership.Id);
            if (access is null)
            {
                return PasskeyUnavailable<AccessTokenDto>();
            }
        }

        return Result.Success(await sessions.IssueAsync(
            user,
            access,
            AuthenticationAssurance.PhishingResistant,
            deviceLabel,
            authenticationMethod: "passkey",
            cancellationToken: cancellationToken));
    }

    public Task<IReadOnlyCollection<PasskeyCredentialDto>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        passkeys.ListAsync(userId, cancellationToken);

    public Task<Result> RevokeAsync(
        Guid userId,
        Guid credentialId,
        CancellationToken cancellationToken = default) =>
        passkeys.RevokeAsync(userId, credentialId, cancellationToken);

    public Task<int> CountActiveAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        passkeys.CountActiveAsync(userId, cancellationToken);

    private static Result<T> PasskeyUnavailable<T>() =>
        Result.Failure<T>(new DomainError(
            "authentication.passkey_unavailable",
            "Passkey authentication is unavailable or invalid."));
}
