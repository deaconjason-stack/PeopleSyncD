using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Domain.Permissions;

/// <summary>
/// Authoritative role-to-permission mapping for tenant access tokens.
/// </summary>
public static class PermissionCatalog
{
    private static readonly IReadOnlyCollection<string> OwnerPermissions = Array.AsReadOnly(
    [
        PermissionNames.OrganizationsRead,
        PermissionNames.OrganizationsWrite,
        PermissionNames.MembershipsRead,
        PermissionNames.MembershipsWrite,
        PermissionNames.EmployeesRead,
        PermissionNames.EmployeesWrite,
    ]);

    private static readonly IReadOnlyCollection<string> AdministratorPermissions = Array.AsReadOnly(
    [
        PermissionNames.OrganizationsRead,
        PermissionNames.OrganizationsWrite,
        PermissionNames.MembershipsRead,
        PermissionNames.MembershipsWrite,
        PermissionNames.EmployeesRead,
        PermissionNames.EmployeesWrite,
    ]);

    private static readonly IReadOnlyCollection<string> ManagerPermissions = Array.AsReadOnly(
    [
        PermissionNames.OrganizationsRead,
        PermissionNames.MembershipsRead,
        PermissionNames.EmployeesRead,
        PermissionNames.EmployeesWrite,
    ]);

    private static readonly IReadOnlyCollection<string> MemberPermissions = Array.AsReadOnly(
    [
        PermissionNames.OrganizationsRead,
        PermissionNames.EmployeesRead,
    ]);

    private static readonly IReadOnlyCollection<string> AuditorPermissions = Array.AsReadOnly(
    [
        PermissionNames.OrganizationsRead,
        PermissionNames.MembershipsRead,
        PermissionNames.EmployeesRead,
    ]);

    private static readonly IReadOnlyCollection<string> NoPermissions = Array.Empty<string>();

    public static IReadOnlyCollection<string> ForRole(TenantRole role) => role switch
    {
        TenantRole.Owner => OwnerPermissions,
        TenantRole.Administrator => AdministratorPermissions,
        TenantRole.Manager => ManagerPermissions,
        TenantRole.Member => MemberPermissions,
        TenantRole.Auditor => AuditorPermissions,
        _ => NoPermissions,
    };
}
