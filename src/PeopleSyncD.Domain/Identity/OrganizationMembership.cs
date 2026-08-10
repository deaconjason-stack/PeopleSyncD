using PeopleSyncD.Domain.Events;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// User authorization membership within one tenant organization.
/// </summary>
public sealed class OrganizationMembership : AggregateRoot<Guid>
{
    private OrganizationMembership()
    {
    }

    private OrganizationMembership(
        Guid id,
        Guid userId,
        Guid organizationId,
        TenantRole role,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        OrganizationId = organizationId;
        Role = role;
        Status = MembershipStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid UserId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public TenantRole Role { get; private set; }

    public MembershipStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<OrganizationMembership> Create(
        Guid userId,
        Guid organizationId,
        TenantRole role,
        DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<OrganizationMembership>(new DomainError(
                "membership.user_required",
                "A membership requires a user identifier."));
        }

        if (organizationId == Guid.Empty)
        {
            return Result.Failure<OrganizationMembership>(new DomainError(
                "membership.organization_required",
                "A membership requires an organization identifier."));
        }

        if (role == TenantRole.None || !Enum.IsDefined(role))
        {
            return Result.Failure<OrganizationMembership>(new DomainError(
                "membership.role_invalid",
                "A recognized tenant role is required."));
        }

        var membership = new OrganizationMembership(
            Guid.NewGuid(),
            userId,
            organizationId,
            role,
            createdAt);
        membership.Raise(new OrganizationMembershipCreatedDomainEvent(
            membership.Id,
            userId,
            organizationId,
            role,
            createdAt));
        return Result.Success(membership);
    }

    public Result ChangeRole(TenantRole role, DateTimeOffset changedAt)
    {
        if (Status != MembershipStatus.Active)
        {
            return Result.Failure(new DomainError(
                "membership.inactive",
                "Only active memberships can change role."));
        }

        if (role == TenantRole.None || !Enum.IsDefined(role))
        {
            return Result.Failure(new DomainError(
                "membership.role_invalid",
                "A recognized tenant role is required."));
        }

        Role = role;
        UpdatedAt = changedAt;
        return Result.Success();
    }

    public Result Suspend(DateTimeOffset changedAt)
    {
        if (Status != MembershipStatus.Active)
        {
            return Result.Failure(new DomainError(
                "membership.not_active",
                "Only active memberships can be suspended."));
        }

        Status = MembershipStatus.Suspended;
        UpdatedAt = changedAt;
        return Result.Success();
    }

    public Result Reactivate(DateTimeOffset changedAt)
    {
        if (Status != MembershipStatus.Suspended)
        {
            return Result.Failure(new DomainError(
                "membership.not_suspended",
                "Only suspended memberships can be reactivated."));
        }

        Status = MembershipStatus.Active;
        UpdatedAt = changedAt;
        return Result.Success();
    }

    public Result Revoke(DateTimeOffset changedAt)
    {
        if (Status == MembershipStatus.Revoked)
        {
            return Result.Failure(new DomainError(
                "membership.already_revoked",
                "The membership has already been revoked."));
        }

        Status = MembershipStatus.Revoked;
        UpdatedAt = changedAt;
        return Result.Success();
    }
}
