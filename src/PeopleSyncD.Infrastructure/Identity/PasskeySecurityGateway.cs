using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Infrastructure.Configuration;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class PasskeySecurityGateway(
    UserManager<ApplicationUser> users,
    ApplicationDbContext database,
    IFido2 fido2,
    PasskeyOptions options,
    IClock clock) : IPasskeySecurityGateway
{
    public async Task<Result<PasskeyCeremonyOptionsDto>> BeginRegistrationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await FindActiveUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unavailable<PasskeyCeremonyOptionsDto>();
        }

        var existing = await ActiveCredentials(user.Id)
            .Select(credential => credential.CredentialId)
            .ToListAsync(cancellationToken);
        var fidoUser = new Fido2User
        {
            Id = user.Id.ToByteArray(),
            Name = user.Email ?? user.UserName ?? user.Id.ToString("D"),
            DisplayName = user.DisplayName,
        };
        var createOptions = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = existing
                .Select(id => new PublicKeyCredentialDescriptor(Base64UrlEncoder.DecodeBytes(id)))
                .ToArray(),
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Required,
            },
            AttestationPreference = AttestationConveyancePreference.None,
        });
        return await StoreCeremonyAsync(
            user.Id,
            "registration",
            createOptions.ToJson(),
            null,
            null,
            cancellationToken);
    }

    public async Task<Result<PasskeyCredentialDto>> CompleteRegistrationAsync(
        Guid userId,
        CompletePasskeyRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var ceremony = await GetCeremonyAsync(request.CeremonyId, cancellationToken);
        if (!IsValidCeremony(ceremony, userId, "registration"))
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        AuthenticatorAttestationRawResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(request.CredentialJson);
        }
        catch (JsonException)
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        if (response is null)
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        RegisteredPublicKeyCredential registered;
        try
        {
            registered = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = response,
                OriginalOptions = CredentialCreateOptions.FromJson(ceremony!.OptionsJson),
                IsCredentialIdUniqueToUserCallback = async (parameters, token) =>
                    !await database.PasskeyCredentials.AnyAsync(
                        credential => credential.CredentialId == Base64UrlEncoder.Encode(parameters.CredentialId),
                        token),
            }, cancellationToken);
        }
        catch (Fido2VerificationException)
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        if (!await TryCompleteCeremonyAsync(ceremony!, cancellationToken))
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        var now = clock.UtcNow;
        var entity = new PasskeyCredential
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CredentialId = Base64UrlEncoder.Encode(registered.Id),
            PublicKey = registered.PublicKey,
            UserHandle = registered.User.Id,
            SignatureCounter = registered.SignCount,
            DisplayName = NormalizeDisplayName(request.DisplayName),
            Transports = string.Join(',', registered.Transports.Select(value => value.ToString())),
            BackupEligible = registered.IsBackupEligible,
            BackedUp = registered.IsBackedUp,
            AaGuid = registered.AaGuid,
            CreatedAt = now,
        };
        await database.PasskeyCredentials.AddAsync(entity, cancellationToken);
        database.SecurityAuditRecords.Add(NewAudit(
            "identity.passkey.registered",
            userId,
            "passkey",
            entity.Id.ToString("D")));
        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Unavailable<PasskeyCredentialDto>();
        }

        return Result.Success(ToDto(entity));
    }

    public async Task<Result<PasskeyCeremonyOptionsDto>> BeginAuthenticationAsync(
        Guid userId,
        string purpose,
        Guid? organizationId = null,
        Guid? membershipId = null,
        CancellationToken cancellationToken = default)
    {
        if (purpose is not ("login" or "step_up")
            || (organizationId is null) != (membershipId is null))
        {
            return Unavailable<PasskeyCeremonyOptionsDto>();
        }

        var user = await FindActiveUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unavailable<PasskeyCeremonyOptionsDto>();
        }

        var credentialIds = await ActiveCredentials(userId)
            .Select(credential => credential.CredentialId)
            .ToListAsync(cancellationToken);
        if (credentialIds.Count == 0)
        {
            return Unavailable<PasskeyCeremonyOptionsDto>();
        }

        var assertionOptions = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = credentialIds
                .Select(id => new PublicKeyCredentialDescriptor(Base64UrlEncoder.DecodeBytes(id)))
                .ToArray(),
            UserVerification = UserVerificationRequirement.Required,
        });
        return await StoreCeremonyAsync(
            userId,
            purpose,
            assertionOptions.ToJson(),
            organizationId,
            membershipId,
            cancellationToken);
    }

    public async Task<Result<PasskeyAuthenticationCompletionDto>> CompleteAuthenticationAsync(
        CompletePasskeyAuthenticationRequest request,
        CancellationToken cancellationToken = default)
    {
        var ceremony = await GetCeremonyAsync(request.CeremonyId, cancellationToken);
        if (ceremony is null
            || ceremony.CompletedAt is not null
            || ceremony.ExpiresAt <= clock.UtcNow
            || ceremony.Purpose is not ("login" or "step_up"))
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        AuthenticatorAssertionRawResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(request.CredentialJson);
        }
        catch (JsonException)
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        if (response is null)
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        var credentialId = Base64UrlEncoder.Encode(response.RawId);
        var credential = await ActiveCredentials(ceremony.UserId)
            .SingleOrDefaultAsync(item => item.CredentialId == credentialId, cancellationToken);
        if (credential is null)
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        VerifyAssertionResult verified;
        try
        {
            verified = await fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = response,
                OriginalOptions = AssertionOptions.FromJson(ceremony.OptionsJson),
                StoredPublicKey = credential.PublicKey,
                StoredSignatureCounter = checked((uint)credential.SignatureCounter),
                IsUserHandleOwnerOfCredentialIdCallback = async (parameters, token) =>
                {
                    var encodedId = Base64UrlEncoder.Encode(parameters.CredentialId);
                    var owner = await ActiveCredentials(ceremony.UserId)
                        .SingleOrDefaultAsync(item => item.CredentialId == encodedId, token);
                    return owner is not null && owner.UserHandle.SequenceEqual(parameters.UserHandle);
                },
            }, cancellationToken);
        }
        catch (Fido2VerificationException)
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        if (!await TryCompleteCeremonyAsync(ceremony, cancellationToken))
        {
            return Unavailable<PasskeyAuthenticationCompletionDto>();
        }

        var now = clock.UtcNow;
        if (database.Database.IsRelational())
        {
            var affected = await database.PasskeyCredentials
                .Where(item => item.Id == credential.Id
                    && item.RevokedAt == null
                    && item.SignatureCounter == credential.SignatureCounter)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(item => item.SignatureCounter, (long)verified.SignCount)
                        .SetProperty(item => item.BackedUp, verified.IsBackedUp)
                        .SetProperty(item => item.LastUsedAt, (DateTimeOffset?)now),
                    cancellationToken);
            if (affected != 1)
            {
                return Unavailable<PasskeyAuthenticationCompletionDto>();
            }
        }
        else
        {
            credential.SignatureCounter = verified.SignCount;
            credential.BackedUp = verified.IsBackedUp;
            credential.LastUsedAt = now;
        }

        database.SecurityAuditRecords.Add(NewAudit(
            "identity.passkey.authenticated",
            ceremony.UserId,
            "passkey",
            credential.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success(new PasskeyAuthenticationCompletionDto(
            ceremony.UserId,
            credential.Id,
            ceremony.Purpose,
            ceremony.OrganizationId,
            ceremony.MembershipId));
    }

    public async Task<IReadOnlyCollection<PasskeyCredentialDto>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var credentials = await ActiveCredentials(userId)
            .AsNoTracking()
            .OrderByDescending(credential => credential.LastUsedAt ?? credential.CreatedAt)
            .ToListAsync(cancellationToken);
        return Array.AsReadOnly(credentials.Select(ToDto).ToArray());
    }

    public async Task<Result> RevokeAsync(
        Guid userId,
        Guid credentialId,
        CancellationToken cancellationToken = default)
    {
        var credential = await ActiveCredentials(userId)
            .SingleOrDefaultAsync(item => item.Id == credentialId, cancellationToken);
        if (credential is null)
        {
            return Result.Failure(new DomainError("passkey.not_found", "The passkey was not found."));
        }

        credential.RevokedAt = clock.UtcNow;
        database.SecurityAuditRecords.Add(NewAudit(
            "identity.passkey.revoked",
            userId,
            "passkey",
            credential.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public Task<int> CountActiveAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ActiveCredentials(userId).CountAsync(cancellationToken);

    private IQueryable<PasskeyCredential> ActiveCredentials(Guid userId) =>
        database.PasskeyCredentials.Where(credential => credential.UserId == userId && credential.RevokedAt == null);

    private async Task<ApplicationUser?> FindActiveUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        return user is { IsActive: true } ? user : null;
    }

    private Task<PasskeyCeremony?> GetCeremonyAsync(
        Guid ceremonyId,
        CancellationToken cancellationToken) =>
        database.PasskeyCeremonies.SingleOrDefaultAsync(item => item.Id == ceremonyId, cancellationToken);

    private bool IsValidCeremony(PasskeyCeremony? ceremony, Guid userId, string purpose) =>
        ceremony is not null
        && ceremony.UserId == userId
        && ceremony.Purpose == purpose
        && ceremony.CompletedAt is null
        && ceremony.ExpiresAt > clock.UtcNow;

    private async Task<Result<PasskeyCeremonyOptionsDto>> StoreCeremonyAsync(
        Guid userId,
        string purpose,
        string optionsJson,
        Guid? organizationId,
        Guid? membershipId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var ceremony = new PasskeyCeremony
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Purpose = purpose,
            OptionsJson = optionsJson,
            OrganizationId = organizationId,
            MembershipId = membershipId,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(Math.Clamp(options.CeremonyMinutes, 1, 15)),
        };
        await database.PasskeyCeremonies.AddAsync(ceremony, cancellationToken);
        database.SecurityAuditRecords.Add(NewAudit(
            $"identity.passkey.{purpose}_started",
            userId,
            "passkey_ceremony",
            ceremony.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success(new PasskeyCeremonyOptionsDto(
            ceremony.Id,
            optionsJson,
            ceremony.ExpiresAt,
            purpose));
    }

    private async Task<bool> TryCompleteCeremonyAsync(
        PasskeyCeremony ceremony,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        if (database.Database.IsRelational())
        {
            return await database.PasskeyCeremonies
                .Where(item => item.Id == ceremony.Id
                    && item.CompletedAt == null
                    && item.ExpiresAt > now)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.CompletedAt, (DateTimeOffset?)now),
                    cancellationToken) == 1;
        }

        if (ceremony.CompletedAt is not null || ceremony.ExpiresAt <= now)
        {
            return false;
        }

        ceremony.CompletedAt = now;
        return true;
    }

    private SecurityAuditRecord NewAudit(
        string eventType,
        Guid userId,
        string targetType,
        string targetId) => new()
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            ActorUserId = userId,
            TargetType = targetType,
            TargetId = targetId,
            OccurredAt = clock.UtcNow,
            MetadataJson = "{}",
        };

    private static PasskeyCredentialDto ToDto(PasskeyCredential credential) => new(
        credential.Id,
        credential.DisplayName,
        credential.CreatedAt,
        credential.LastUsedAt,
        credential.BackupEligible,
        credential.BackedUp);

    private static string NormalizeDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Passkey";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 200 ? trimmed : trimmed[..200];
    }

    private static Result<T> Unavailable<T>() =>
        Result.Failure<T>(new DomainError(
            "authentication.passkey_unavailable",
            "Passkey authentication is unavailable or invalid."));
}
