using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// Single-use invitation to join an organization.
/// </summary>
public sealed class OrganizationInvitation : AggregateRoot<Guid>
{
    private OrganizationInvitation()
    {
        Email = string.Empty;
        DisplayName = string.Empty;
        TokenHash = string.Empty;
    }

    private OrganizationInvitation(
        Guid id,
        Guid organizationId,
        Guid invitedByUserId,
        string email,
        string displayName,
        TenantRole role,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
        : base(id)
    {
        OrganizationId = organizationId;
        InvitedByUserId = invitedByUserId;
        Email = email;
        DisplayName = displayName;
        Role = role;
        TokenHash = tokenHash;
        Status = InvitationStatus.Pending;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid OrganizationId { get; private set; }

    public Guid InvitedByUserId { get; private set; }

    public string Email { get; private set; }

    public string DisplayName { get; private set; }

    public TenantRole Role { get; private set; }

    public string TokenHash { get; private set; }

    public InvitationStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public static Result<OrganizationInvitation> Create(
        Guid organizationId,
        Guid invitedByUserId,
        string email,
        string displayName,
        TenantRole role,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (organizationId == Guid.Empty || invitedByUserId == Guid.Empty)
        {
            return Result.Failure<OrganizationInvitation>(new DomainError(
                "invitation.context_invalid",
                "An invitation requires an organization and inviting user."));
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result.Failure<OrganizationInvitation>(new DomainError(
                "invitation.identity_invalid",
                "An invitation requires an email address and secure token."));
        }

        if (role is TenantRole.None or TenantRole.Owner || !Enum.IsDefined(role))
        {
            return Result.Failure<OrganizationInvitation>(new DomainError(
                "invitation.role_invalid",
                "Invitations may assign administrator, manager, member, or auditor roles."));
        }

        if (expiresAt <= createdAt)
        {
            return Result.Failure<OrganizationInvitation>(new DomainError(
                "invitation.expiration_invalid",
                "Invitation expiration must be after creation."));
        }

        return Result.Success(new OrganizationInvitation(
            Guid.NewGuid(),
            organizationId,
            invitedByUserId,
            email.Trim().ToLowerInvariant(),
            displayName.Trim(),
            role,
            tokenHash,
            createdAt,
            expiresAt));
    }

    public Result Accept(DateTimeOffset acceptedAt)
    {
        if (Status != InvitationStatus.Pending || acceptedAt > ExpiresAt)
        {
            Status = acceptedAt > ExpiresAt ? InvitationStatus.Expired : Status;
            return Result.Failure(new DomainError(
                "invitation.unavailable",
                "The invitation is no longer available."));
        }

        Status = InvitationStatus.Accepted;
        AcceptedAt = acceptedAt;
        return Result.Success();
    }

    public Result Revoke(DateTimeOffset revokedAt)
    {
        if (Status != InvitationStatus.Pending)
        {
            return Result.Failure(new DomainError(
                "invitation.not_pending",
                "Only pending invitations can be revoked."));
        }

        Status = InvitationStatus.Revoked;
        RevokedAt = revokedAt;
        return Result.Success();
    }
}
