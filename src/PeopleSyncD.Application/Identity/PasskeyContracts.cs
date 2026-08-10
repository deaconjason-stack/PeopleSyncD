namespace PeopleSyncD.Application.Identity;

public sealed record PasskeyCeremonyOptionsDto(
    Guid CeremonyId,
    string PublicKeyOptionsJson,
    DateTimeOffset ExpiresAt,
    string Purpose);

public sealed record BeginPasskeyAuthenticationRequest(string Email);

public sealed record CompletePasskeyRegistrationRequest(
    Guid CeremonyId,
    string CredentialJson,
    string? DisplayName = null);

public sealed record CompletePasskeyAuthenticationRequest(
    Guid CeremonyId,
    string CredentialJson);

public sealed record PasskeyCredentialDto(
    Guid Id,
    string DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt,
    bool BackupEligible,
    bool BackedUp);

public sealed record PasskeyAuthenticationCompletionDto(
    Guid UserId,
    Guid CredentialId,
    string Purpose,
    Guid? OrganizationId,
    Guid? MembershipId);
