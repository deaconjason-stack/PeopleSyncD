using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class UpdateMembershipService(
    IOrganizationMembershipRepository memberships,
    IAuditRecorder audit,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result> ExecuteAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid membershipId,
        UpdateMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await memberships.GetActiveAsync(actorUserId, organizationId, cancellationToken);
        var target = await memberships.GetByIdAsync(membershipId, cancellationToken);
        if (actor is null || target is null || target.OrganizationId != organizationId)
        {
            return Result.Failure(new DomainError("membership.not_found", "The membership is unavailable."));
        }

        if (actor.Role is not (TenantRole.Owner or TenantRole.Administrator))
        {
            return Result.Failure(new DomainError("membership.forbidden", "Membership administration is not allowed."));
        }

        if (target.UserId == actorUserId)
        {
            return Result.Failure(new DomainError(
                "membership.self_change_forbidden",
                "Use a separate ownership or account-security workflow for changes to your own membership."));
        }

        if (target.Role == TenantRole.Owner && actor.Role != TenantRole.Owner)
        {
            return Result.Failure(new DomainError("membership.owner_protected", "Only an owner can modify another owner."));
        }

        if (request.Role == TenantRole.Owner)
        {
            return Result.Failure(new DomainError(
                "membership.owner_promotion_requires_transfer",
                "Owner promotion requires the dedicated ownership-transfer workflow."));
        }

        var removesOwner = target.Role == TenantRole.Owner
            && (request.Role is not null && request.Role != TenantRole.Owner
                || request.Status is MembershipStatus.Suspended or MembershipStatus.Revoked);
        if (removesOwner && await memberships.CountActiveOwnersAsync(organizationId, cancellationToken) <= 1)
        {
            return Result.Failure(new DomainError(
                "membership.final_owner_protected",
                "The final active owner cannot be demoted, suspended, or revoked."));
        }

        if (request.Role is not null && request.Role != target.Role)
        {
            var roleResult = target.ChangeRole(request.Role.Value, clock.UtcNow);
            if (roleResult.IsFailure)
            {
                return roleResult;
            }
        }

        if (request.Status is not null && request.Status != target.Status)
        {
            var statusResult = request.Status.Value switch
            {
                MembershipStatus.Active => target.Reactivate(clock.UtcNow),
                MembershipStatus.Suspended => target.Suspend(clock.UtcNow),
                MembershipStatus.Revoked => target.Revoke(clock.UtcNow),
                _ => Result.Failure(new DomainError("membership.status_invalid", "The requested status is invalid.")),
            };
            if (statusResult.IsFailure)
            {
                return statusResult;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "membership.updated",
            actorUserId,
            organizationId,
            "membership",
            target.Id.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string>
            {
                ["role"] = target.Role.ToString(),
                ["status"] = target.Status.ToString(),
            }), cancellationToken);
        return Result.Success();
    }
}
