namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// Lifecycle state for an organization membership.
/// </summary>
public enum MembershipStatus
{
    None = 0,
    Active = 1,
    Suspended = 2,
    Revoked = 3,
}
