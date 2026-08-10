namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class RefreshSession
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public Guid? MembershipId { get; set; }

    public Guid? ParentSessionId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset LastSeenAt { get; set; }

    public string AssuranceLevel { get; set; } = "pwd";

    public DateTimeOffset AuthenticatedAt { get; set; }

    public string AuthenticationMethod { get; set; } = "pwd";

    public string? DeviceLabel { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevokeReason { get; set; }
}
