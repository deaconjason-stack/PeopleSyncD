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
    DateTimeOffset? RefreshTokenExpiresAt = null,
    string AssuranceLevel = "pwd",
    Guid? SessionFamilyId = null);

/// <summary>
/// Result produced by atomic owner-and-tenant provisioning.
/// </summary>
public sealed record ProvisionedTenantDto(
    IdentityUserDto User,
    OrganizationAccessDto Access);

public sealed record RefreshTokenDto(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid FamilyId = default);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RefreshRotationDto(
    Guid FamilyId,
    Guid UserId,
    Guid? OrganizationId,
    Guid? MembershipId,
    RefreshTokenDto Replacement,
    string AssuranceLevel = "pwd",
    string? DeviceLabel = null,
    DateTimeOffset? AuthenticatedAt = null,
    string AuthenticationMethod = "pwd");

public sealed record ConfirmEmailRequest(Guid UserId, string Token);

public sealed record AccountSecurityDto(
    Guid UserId,
    bool EmailConfirmed,
    bool MfaEnabled,
    bool PasswordOnlyLoginAllowed,
    int RecoveryCodesRemaining = 0);

public sealed record MfaChallengeDto(
    string ChallengeToken,
    DateTimeOffset ExpiresAt,
    IReadOnlyCollection<string> Methods,
    string Purpose);

public sealed record MfaChallengeRequest(
    string ChallengeToken,
    string Method,
    string Code);

public sealed record MfaChallengeCompletionDto(
    Guid UserId,
    string Purpose,
    string Method,
    Guid? OrganizationId,
    Guid? MembershipId);

public sealed record LoginOutcomeDto(
    AccessTokenDto? Session,
    MfaChallengeDto? Challenge);

public sealed record MfaTotpEnrollmentDto(
    string ManualEntryKey,
    string OtpauthUri);

public sealed record ConfirmTotpEnrollmentRequest(string Code);

public sealed record RecoveryCodeBatchDto(
    IReadOnlyCollection<string> RecoveryCodes,
    DateTimeOffset GeneratedAt);

public sealed record SessionSummaryDto(
    Guid FamilyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastSeenAt,
    string AssuranceLevel,
    string? DeviceLabel,
    bool IsCurrent,
    DateTimeOffset? AuthenticatedAt = null,
    string AuthenticationMethod = "pwd");

public sealed record SecurityEventDto(
    string EventType,
    DateTimeOffset OccurredAt,
    string TargetType,
    string TargetId);
