using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Issues short-lived access tokens for authenticated identities and tenant contexts.
/// </summary>
public interface IAccessTokenIssuer
{
    AccessTokenDto Issue(
        IdentityUserDto user,
        OrganizationAccessDto? access = null,
        string assuranceLevel = "pwd",
        Guid? sessionFamilyId = null,
        DateTimeOffset? authenticatedAt = null,
        string? authenticationMethod = null);
}
