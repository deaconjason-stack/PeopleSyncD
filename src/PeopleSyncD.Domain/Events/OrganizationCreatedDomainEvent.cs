using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Events;

/// <summary>
/// Raised when an organization aggregate is created.
/// </summary>
public sealed record OrganizationCreatedDomainEvent(
    Guid OrganizationId,
    string Name,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
