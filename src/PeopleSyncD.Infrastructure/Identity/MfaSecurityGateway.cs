using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class MfaSecurityGateway(
    UserManager<ApplicationUser> users,
    ApplicationDbContext database,
    IClock clock) : IMfaSecurityGateway
{
    private const int RecoveryCodeCount = 10;
    private const int MaxChallengeAttempts = 5;
    private const int TotpWindow = 2;
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);
    private static readonly IReadOnlyCollection<string> ChallengeMethods =
        Array.AsReadOnly(new[] { "totp", "recovery_code" });

    public async Task<Result<MfaTotpEnrollmentDto>> BeginTotpEnrollmentAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return UserUnavailable<MfaTotpEnrollmentDto>();
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Failure<MfaTotpEnrollmentDto>(new DomainError(
                "mfa.already_enabled",
                "Multi-factor authentication is already enabled."));
        }

        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            var reset = await users.ResetAuthenticatorKeyAsync(user);
            if (!reset.Succeeded)
            {
                return IdentityFailure<MfaTotpEnrollmentDto>(reset, "mfa.enrollment_failed");
            }

            key = await users.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return Result.Failure<MfaTotpEnrollmentDto>(new DomainError(
                "mfa.enrollment_failed",
                "An authenticator key could not be generated."));
        }

        var normalizedKey = key.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        var account = Uri.EscapeDataString(user.Email ?? user.UserName ?? user.Id.ToString("D"));
        var issuer = Uri.EscapeDataString("PeopleSyncD");
        var uri = $"otpauth://totp/{issuer}:{account}?secret={normalizedKey}&issuer={issuer}&digits=6&period=30&algorithm=SHA1";
        return Result.Success(new MfaTotpEnrollmentDto(normalizedKey, uri));
    }

    public async Task<Result<RecoveryCodeBatchDto>> ConfirmTotpEnrollmentAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return UserUnavailable<RecoveryCodeBatchDto>();
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Failure<RecoveryCodeBatchDto>(new DomainError(
                "mfa.already_enabled",
                "Multi-factor authentication is already enabled."));
        }

        var matchedCounter = await MatchTotpCounterAsync(user, code);
        if (matchedCounter is null)
        {
            return Result.Failure<RecoveryCodeBatchDto>(new DomainError(
                "mfa.invalid_code",
                "The authenticator code is invalid."));
        }

        var transaction = database.Database.IsRelational()
            ? await database.Database.BeginTransactionAsync(cancellationToken)
            : null;
        try
        {
            if (!await TryAdvanceTotpCounterAsync(user.Id, matchedCounter.Value, cancellationToken))
            {
                database.SecurityAuditRecords.Add(NewAudit(
                    "identity.mfa.enrollment_replay_denied",
                    user.Id,
                    "user",
                    user.Id.ToString("D")));
                await database.SaveChangesAsync(cancellationToken);
                return Result.Failure<RecoveryCodeBatchDto>(new DomainError(
                    "mfa.invalid_code",
                    "The authenticator code is invalid."));
            }

            var enabled = await users.SetTwoFactorEnabledAsync(user, true);
            if (!enabled.Succeeded)
            {
                return IdentityFailure<RecoveryCodeBatchDto>(enabled, "mfa.enable_failed");
            }

            var batch = await ReplaceRecoveryCodesAsync(user.Id, cancellationToken);
            await RevokeUserSessionsAsync(user.Id, "mfa_enabled", cancellationToken);
            database.SecurityAuditRecords.Add(NewAudit(
                "identity.mfa.enabled",
                user.Id,
                "user",
                user.Id.ToString("D")));
            await database.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return Result.Success(batch);
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<Result<RecoveryCodeBatchDto>> RegenerateRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return UserUnavailable<RecoveryCodeBatchDto>();
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Failure<RecoveryCodeBatchDto>(new DomainError(
                "mfa.not_enabled",
                "Multi-factor authentication is not enabled."));
        }

        var batch = await ReplaceRecoveryCodesAsync(user.Id, cancellationToken);
        database.SecurityAuditRecords.Add(NewAudit(
            "identity.mfa.recovery_codes_regenerated",
            user.Id,
            "user",
            user.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success(batch);
    }

    public async Task<Result<MfaChallengeDto>> CreateChallengeAsync(
        Guid userId,
        string purpose,
        Guid? organizationId = null,
        Guid? membershipId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return UserUnavailable<MfaChallengeDto>();
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Failure<MfaChallengeDto>(new DomainError(
                "mfa.not_enabled",
                "Multi-factor authentication is not enabled."));
        }

        if (purpose is not ("login" or "step_up"))
        {
            return Result.Failure<MfaChallengeDto>(new DomainError(
                "mfa.challenge_purpose_invalid",
                "The multi-factor challenge purpose is invalid."));
        }

        if ((organizationId is null) != (membershipId is null))
        {
            return Result.Failure<MfaChallengeDto>(new DomainError(
                "mfa.challenge_context_invalid",
                "Organization and membership challenge context must be provided together."));
        }

        var raw = CreateOpaqueSecret(32);
        var now = clock.UtcNow;
        var challenge = new MfaChallenge
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = purpose,
            TokenHash = Hash(raw),
            OrganizationId = organizationId,
            MembershipId = membershipId,
            CreatedAt = now,
            ExpiresAt = now.Add(ChallengeLifetime),
        };
        await database.MfaChallenges.AddAsync(challenge, cancellationToken);
        database.SecurityAuditRecords.Add(NewAudit(
            "identity.mfa.challenge_created",
            user.Id,
            "mfa_challenge",
            challenge.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success(new MfaChallengeDto(
            raw,
            challenge.ExpiresAt,
            ChallengeMethods,
            purpose));
    }

    public async Task<Result<MfaChallengeCompletionDto>> CompleteChallengeAsync(
        MfaChallengeRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(request.ChallengeToken)
            || string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.Method))
        {
            return InvalidChallenge();
        }

        var tokenHash = Hash(request.ChallengeToken);
        var challenge = await database.MfaChallenges.SingleOrDefaultAsync(
            item => item.TokenHash == tokenHash,
            cancellationToken);
        if (challenge is null
            || challenge.CompletedAt is not null
            || challenge.ExpiresAt <= clock.UtcNow
            || challenge.FailedAttempts >= MaxChallengeAttempts)
        {
            return InvalidChallenge();
        }

        var user = await users.FindByIdAsync(challenge.UserId.ToString("D"));
        if (user is null || !user.IsActive || !user.TwoFactorEnabled)
        {
            return UserUnavailable<MfaChallengeCompletionDto>();
        }

        var method = request.Method.Trim().ToLowerInvariant();
        var factor = await VerifyFactorAsync(user, method, request.Code, cancellationToken);
        if (factor != FactorVerification.Verified)
        {
            await RecordFailedAttemptAsync(challenge, cancellationToken);
            database.SecurityAuditRecords.Add(NewAudit(
                factor == FactorVerification.Replay
                    ? "identity.mfa.totp_replay_denied"
                    : "identity.mfa.challenge_failed",
                user.Id,
                "mfa_challenge",
                challenge.Id.ToString("D")));
            await database.SaveChangesAsync(cancellationToken);
            return Result.Failure<MfaChallengeCompletionDto>(new DomainError(
                "mfa.challenge_invalid",
                "The multi-factor challenge or code is invalid."));
        }

        if (!await TryCompleteChallengeAsync(challenge, cancellationToken))
        {
            return InvalidChallenge();
        }

        database.SecurityAuditRecords.Add(NewAudit(
            "identity.mfa.challenge_completed",
            user.Id,
            "mfa_challenge",
            challenge.Id.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success(new MfaChallengeCompletionDto(
            user.Id,
            challenge.Purpose,
            method,
            challenge.OrganizationId,
            challenge.MembershipId));
    }

    public Task<int> GetRecoveryCodeCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        database.MfaRecoveryCodes.CountAsync(
            code => code.UserId == userId && code.UsedAt == null && code.RevokedAt == null,
            cancellationToken);

    public async Task<IReadOnlyCollection<SecurityEventDto>> ListSecurityEventsAsync(
        Guid userId,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 50);
        var events = await database.SecurityAuditRecords
            .AsNoTracking()
            .Where(item => item.ActorUserId == userId)
            .OrderByDescending(item => item.OccurredAt)
            .Take(take)
            .Select(item => new SecurityEventDto(
                item.EventType,
                item.OccurredAt,
                item.TargetType,
                item.TargetId))
            .ToListAsync(cancellationToken);
        return events.AsReadOnly();
    }

    private async Task<FactorVerification> VerifyFactorAsync(
        ApplicationUser user,
        string method,
        string code,
        CancellationToken cancellationToken)
    {
        if (method == "recovery_code")
        {
            return await ConsumeRecoveryCodeAsync(user.Id, code, cancellationToken)
                ? FactorVerification.Verified
                : FactorVerification.Invalid;
        }

        if (method != "totp")
        {
            return FactorVerification.Invalid;
        }

        var counter = await MatchTotpCounterAsync(user, code);
        if (counter is null)
        {
            return FactorVerification.Invalid;
        }

        return await TryAdvanceTotpCounterAsync(user.Id, counter.Value, cancellationToken)
            ? FactorVerification.Verified
            : FactorVerification.Replay;
    }

    private async Task<long?> MatchTotpCounterAsync(ApplicationUser user, string rawCode)
    {
        var normalized = NormalizeTotp(rawCode);
        if (normalized.Length != 6
            || !int.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var suppliedCode))
        {
            return null;
        }

        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var keyBytes = DecodeBase32(key);
        var currentCounter = clock.UtcNow.ToUnixTimeSeconds() / 30;
        long? matched = null;
        for (var offset = -TotpWindow; offset <= TotpWindow; offset++)
        {
            var counter = currentCounter + offset;
            if (counter < 0 || ComputeTotpCode(keyBytes, counter) != suppliedCode)
            {
                continue;
            }

            if (matched is null || counter > matched.Value)
            {
                matched = counter;
            }
        }

        return matched;
    }

    private async Task<bool> TryAdvanceTotpCounterAsync(
        Guid userId,
        long counter,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        if (database.Database.IsRelational())
        {
            var affected = await database.Database.ExecuteSqlInterpolatedAsync($$"""
                INSERT INTO mfa_totp_states ("UserId", "LastAcceptedCounter", "EnrolledAt", "UpdatedAt")
                VALUES ({{userId}}, {{counter}}, {{now}}, {{now}})
                ON CONFLICT ("UserId") DO UPDATE
                SET "LastAcceptedCounter" = EXCLUDED."LastAcceptedCounter",
                    "UpdatedAt" = EXCLUDED."UpdatedAt"
                WHERE mfa_totp_states."LastAcceptedCounter" < EXCLUDED."LastAcceptedCounter";
                """, cancellationToken);
            return affected == 1;
        }

        var state = await database.MfaTotpStates.SingleOrDefaultAsync(
            item => item.UserId == userId,
            cancellationToken);
        if (state is null)
        {
            await database.MfaTotpStates.AddAsync(new MfaTotpState
            {
                UserId = userId,
                LastAcceptedCounter = counter,
                EnrolledAt = now,
                UpdatedAt = now,
            }, cancellationToken);
            return true;
        }

        if (counter <= state.LastAcceptedCounter)
        {
            return false;
        }

        state.LastAcceptedCounter = counter;
        state.UpdatedAt = now;
        return true;
    }

    private async Task<RecoveryCodeBatchDto> ReplaceRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var existing = await database.MfaRecoveryCodes
            .Where(code => code.UserId == userId && code.RevokedAt == null && code.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var code in existing)
        {
            code.RevokedAt = clock.UtcNow;
        }

        var batchId = Guid.NewGuid();
        var generated = Enumerable.Range(0, RecoveryCodeCount)
            .Select(_ => CreateRecoveryCode())
            .ToArray();
        foreach (var raw in generated)
        {
            await database.MfaRecoveryCodes.AddAsync(new MfaRecoveryCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BatchId = batchId,
                CodeHash = Hash(NormalizeRecoveryCode(raw)),
                CreatedAt = clock.UtcNow,
            }, cancellationToken);
        }

        return new RecoveryCodeBatchDto(Array.AsReadOnly(generated), clock.UtcNow);
    }

    private async Task<bool> ConsumeRecoveryCodeAsync(
        Guid userId,
        string rawCode,
        CancellationToken cancellationToken)
    {
        var hash = Hash(NormalizeRecoveryCode(rawCode));
        var now = clock.UtcNow;
        if (database.Database.IsRelational())
        {
            var affected = await database.MfaRecoveryCodes
                .Where(item => item.UserId == userId
                    && item.CodeHash == hash
                    && item.UsedAt == null
                    && item.RevokedAt == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.UsedAt, (DateTimeOffset?)now),
                    cancellationToken);
            return affected == 1;
        }

        var code = await database.MfaRecoveryCodes.SingleOrDefaultAsync(
            item => item.UserId == userId
                && item.CodeHash == hash
                && item.UsedAt == null
                && item.RevokedAt == null,
            cancellationToken);
        if (code is null)
        {
            return false;
        }

        code.UsedAt = now;
        return true;
    }

    private async Task RecordFailedAttemptAsync(
        MfaChallenge challenge,
        CancellationToken cancellationToken)
    {
        if (database.Database.IsRelational())
        {
            await database.MfaChallenges
                .Where(item => item.Id == challenge.Id
                    && item.CompletedAt == null
                    && item.ExpiresAt > clock.UtcNow
                    && item.FailedAttempts < MaxChallengeAttempts)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.FailedAttempts, item => item.FailedAttempts + 1),
                    cancellationToken);
            return;
        }

        challenge.FailedAttempts++;
    }

    private async Task<bool> TryCompleteChallengeAsync(
        MfaChallenge challenge,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        if (database.Database.IsRelational())
        {
            var affected = await database.MfaChallenges
                .Where(item => item.Id == challenge.Id
                    && item.CompletedAt == null
                    && item.ExpiresAt > now
                    && item.FailedAttempts < MaxChallengeAttempts)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(item => item.CompletedAt, (DateTimeOffset?)now),
                    cancellationToken);
            return affected == 1;
        }

        if (challenge.CompletedAt is not null
            || challenge.ExpiresAt <= now
            || challenge.FailedAttempts >= MaxChallengeAttempts)
        {
            return false;
        }

        challenge.CompletedAt = now;
        return true;
    }

    private async Task RevokeUserSessionsAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var active = await database.RefreshSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in active)
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }
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

    private static int ComputeTotpCode(byte[] key, long counter)
    {
        Span<byte> counterBytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counterBytes, counter);
#pragma warning disable CA5350 // RFC 6238 interoperable authenticator profile required by ASP.NET Core Identity.
        var digest = HMACSHA1.HashData(key, counterBytes);
#pragma warning restore CA5350
        var offset = digest[^1] & 0x0f;
        var binary = ((digest[offset] & 0x7f) << 24)
            | (digest[offset + 1] << 16)
            | (digest[offset + 2] << 8)
            | digest[offset + 3];
        return binary % 1_000_000;
    }

    private static byte[] DecodeBase32(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        var buffer = 0;
        var bits = 0;
        foreach (var raw in value)
        {
            if (raw is '=' or ' ' or '-')
            {
                continue;
            }

            var index = alphabet.IndexOf(char.ToUpperInvariant(raw));
            if (index < 0)
            {
                return Array.Empty<byte>();
            }

            buffer = (buffer << 5) | index;
            bits += 5;
            if (bits < 8)
            {
                continue;
            }

            bits -= 8;
            output.Add((byte)(buffer >> bits));
            buffer &= bits == 0 ? 0 : (1 << bits) - 1;
        }

        return output.ToArray();
    }

    private static string NormalizeTotp(string code) =>
        code.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

    private static string NormalizeRecoveryCode(string code) =>
        code.Trim().Replace("-", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string CreateRecoveryCode()
    {
        var value = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        return string.Join('-', Enumerable.Range(0, 4).Select(index => value.Substring(index * 4, 4)));
    }

    private static string CreateOpaqueSecret(int bytes)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));
        return raw.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static Result<T> UserUnavailable<T>() =>
        Result.Failure<T>(new DomainError(
            "authentication.user_unavailable",
            "The account is unavailable."));

    private static Result<T> IdentityFailure<T>(IdentityResult result, string code) =>
        Result.Failure<T>(new DomainError(
            code,
            string.Join(" ", result.Errors.Select(error => error.Description))));

    private static Result<MfaChallengeCompletionDto> InvalidChallenge() =>
        Result.Failure<MfaChallengeCompletionDto>(new DomainError(
            "mfa.challenge_invalid",
            "The multi-factor challenge is invalid or expired."));

    private enum FactorVerification
    {
        Invalid,
        Verified,
        Replay,
    }
}
