using System.Text.Json;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class DatabaseAuditRecorder(ApplicationDbContext database) : IAuditRecorder
{
    public async Task RecordAsync(
        SecurityAuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        database.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            Id = Guid.NewGuid(),
            EventType = auditEvent.EventType,
            ActorUserId = auditEvent.ActorUserId,
            OrganizationId = auditEvent.OrganizationId,
            TargetType = auditEvent.TargetType,
            TargetId = auditEvent.TargetId,
            OccurredAt = auditEvent.OccurredAt,
            MetadataJson = JsonSerializer.Serialize(auditEvent.Metadata ?? new Dictionary<string, string>()),
        });
        await database.SaveChangesAsync(cancellationToken);
    }
}
