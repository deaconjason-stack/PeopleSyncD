namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// Organization-scoped roles recognized by the platform authorization model.
/// </summary>
public enum TenantRole
{
    None = 0,
    Owner = 1,
    Administrator = 2,
    Manager = 3,
    Member = 4,
    Auditor = 5,
}
