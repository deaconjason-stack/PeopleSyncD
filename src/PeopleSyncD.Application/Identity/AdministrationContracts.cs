using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Identity;

public sealed record CreateInvitationRequest(string Email, string DisplayName, TenantRole Role);

public sealed record InvitationDto(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string DisplayName,
    TenantRole Role,
    InvitationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed record AcceptInvitationRequest(string Token, string DisplayName, string Password);

public sealed record MembershipAdminDto(
    Guid MembershipId,
    Guid UserId,
    Guid OrganizationId,
    string DisplayName,
    string Email,
    TenantRole Role,
    MembershipStatus Status,
    bool EmailConfirmed,
    bool MfaEnabled);

public sealed record UpdateMembershipRequest(TenantRole? Role, MembershipStatus? Status);

public sealed record IdentityAdministrationUserDto(
    Guid Id,
    string DisplayName,
    string Email,
    bool EmailConfirmed,
    bool IsActive,
    bool MfaEnabled);

public sealed record InvitationSecret(string Token, string Hash);

public sealed record SecurityAuditEvent(
    string EventType,
    Guid? ActorUserId,
    Guid? OrganizationId,
    string TargetType,
    string TargetId,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, string>? Metadata = null);
