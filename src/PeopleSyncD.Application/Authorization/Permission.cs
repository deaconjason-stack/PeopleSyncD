namespace PeopleSyncD.Application.Authorization;

public enum Permission
{
    OrganizationsRead = 1,
    OrganizationsWrite = 2,
    PeopleRead = 3,
    PeopleWrite = 4,
    Administration = 5
}

public static class RolePermissions
{
    public static bool HasPermission(string role, Permission permission) => role switch
    {
        "Owner" => true,
        "Administrator" => true,
        "Manager" => permission is Permission.PeopleRead or Permission.PeopleWrite or Permission.OrganizationsRead,
        "Member" => permission is Permission.PeopleRead or Permission.OrganizationsRead,
        _ => false
    };
}
