namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class PasskeyCeremony
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string OptionsJson { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }

    public Guid? MembershipId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
