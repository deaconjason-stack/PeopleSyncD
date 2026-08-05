using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Events;

/// <summary>
/// Raised when a user receives an organization membership.
/// </summary>
public sealed record OrganizationMembershipCreatedDomainEvent(
    Guid MembershipId,
    Guid UserId,
    Guid OrganizationId,
    TenantRole Role,
    DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
