using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Application.Interfaces;

public interface IAuditRecorder
{
    Task RecordAsync(SecurityAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
