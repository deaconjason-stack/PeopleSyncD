namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// Lifecycle state for an organization invitation.
/// </summary>
public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Revoked = 3,
    Expired = 4,
}
