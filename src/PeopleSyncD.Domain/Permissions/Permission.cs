namespace PeopleSyncD.Domain.Permissions;

/// <summary>
/// Stable permission identifiers used by authorization policies.
/// </summary>
public static class PermissionNames
{
    public const string OrganizationsRead = "organizations.read";
    public const string OrganizationsWrite = "organizations.write";
    public const string EmployeesRead = "employees.read";
    public const string EmployeesWrite = "employees.write";
}
