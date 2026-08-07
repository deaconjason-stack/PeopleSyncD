namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class SecurityAuditRecord
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid? ActorUserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string MetadataJson { get; set; } = "{}";
}
