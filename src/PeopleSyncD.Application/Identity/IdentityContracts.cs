using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Tenant and owner registration request.
/// </summary>
public sealed record RegisterTenantRequest(
    string OrganizationName,
    string OrganizationSlug,
    string DisplayName,
    string Email,
    string Password);

/// <summary>
/// Password authentication request.
/// </summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Request to exchange a user token for a tenant-scoped token.
/// </summary>
public sealed record SelectOrganizationRequest(Guid OrganizationId);

/// <summary>
/// Authenticated platform user projected outside ASP.NET Core Identity.
/// </summary>
public sealed record IdentityUserDto(
    Guid Id,
    string DisplayName,
    string Email,
    bool EmailConfirmed,
    bool IsActive,
    bool MfaEnabled = false);

/// <summary>
/// Organization membership visible to the authenticated user.
/// </summary>
public sealed record OrganizationAccessDto(
    Guid MembershipId,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationSlug,
    TenantRole Role,
    MembershipStatus Status);

/// <summary>
/// Tenant context carried by a tenant-scoped access token.
/// </summary>
public sealed record TenantContextDto(
    Guid MembershipId,
    Guid OrganizationId,
    string OrganizationName,
    string OrganizationSlug,
    TenantRole Role,
    IReadOnlyCollection<string> Permissions);

/// <summary>
/// Access and rotating refresh token response.
/// </summary>
public sealed record AccessTokenDto(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    IdentityUserDto User,
    TenantContextDto? Tenant,
    string? RefreshToken = null,
    DateTimeOffset? RefreshTokenExpiresAt = null);

/// <summary>
/// Result produced by atomic owner-and-tenant provisioning.
/// </summary>
public sealed record ProvisionedTenantDto(
    IdentityUserDto User,
    OrganizationAccessDto Access);

public sealed record RefreshTokenDto(string Token, DateTimeOffset ExpiresAt);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RefreshRotationDto(
    Guid FamilyId,
    Guid UserId,
    Guid? OrganizationId,
    Guid? MembershipId,
    RefreshTokenDto Replacement);

public sealed record ConfirmEmailRequest(Guid UserId, string Token);

public sealed record AccountSecurityDto(
    Guid UserId,
    bool EmailConfirmed,
    bool MfaEnabled,
    bool PasswordOnlyLoginAllowed);
