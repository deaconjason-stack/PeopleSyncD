using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class RefreshSessionGateway(
    ApplicationDbContext database,
    IClock clock) : IRefreshSessionGateway
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(30);

    public async Task<RefreshTokenDto> IssueAsync(
        Guid userId,
        Guid? organizationId,
        Guid? membershipId,
        Guid? familyId = null,
        string assuranceLevel = "pwd",
        string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var issued = CreateSession(
            userId,
            organizationId,
            membershipId,
            familyId ?? Guid.NewGuid(),
            null,
            NormalizeAssurance(assuranceLevel),
            NormalizeDeviceLabel(deviceLabel));
        await database.RefreshSessions.AddAsync(issued.Session, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        return issued.Token;
    }

    public async Task<Result<RefreshRotationDto>> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);
        var current = await database.RefreshSessions.SingleOrDefaultAsync(
            session => session.TokenHash == hash,
            cancellationToken);
        if (current is null)
        {
            return Invalid();
        }

        if (current.UsedAt is not null)
        {
            await RevokeFamilyInternalAsync(current.FamilyId, "reuse_detected", cancellationToken);
            database.SecurityAuditRecords.Add(NewAudit(
                "identity.refresh.reuse_detected",
                current.UserId,
                current.OrganizationId,
                "refresh_family",
                current.FamilyId.ToString("D")));
            await database.SaveChangesAsync(cancellationToken);
            return Result.Failure<RefreshRotationDto>(new DomainError(
                "refresh.reuse_detected",
                "The refresh-token family has been revoked. Reauthentication is required."));
        }

        if (current.RevokedAt is not null || current.ExpiresAt <= clock.UtcNow)
        {
            return Invalid();
        }

        current.UsedAt = clock.UtcNow;
        current.LastSeenAt = clock.UtcNow;
        var replacement = CreateSession(
            current.UserId,
            current.OrganizationId,
            current.MembershipId,
            current.FamilyId,
            current.Id,
            current.AssuranceLevel,
            current.DeviceLabel);
        await database.RefreshSessions.AddAsync(replacement.Session, cancellationToken);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();
            await RevokeFamilyInternalAsync(current.FamilyId, "concurrent_rotation", cancellationToken);
            await database.SaveChangesAsync(cancellationToken);
            return Result.Failure<RefreshRotationDto>(new DomainError(
                "refresh.concurrent_rotation",
                "The refresh session changed concurrently. Reauthentication is required."));
        }

        return Result.Success(new RefreshRotationDto(
            current.FamilyId,
            current.UserId,
            current.OrganizationId,
            current.MembershipId,
            replacement.Token,
            current.AssuranceLevel,
            current.DeviceLabel));
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        await RevokeFamilyInternalAsync(familyId, reason, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeForMembershipAsync(
        Guid membershipId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var sessions = await database.RefreshSessions
            .Where(session => session.MembershipId == membershipId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var sessions = await database.RefreshSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result> RevokeUserFamilyAsync(
        Guid userId,
        Guid familyId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var sessions = await database.RefreshSessions
            .Where(session => session.UserId == userId && session.FamilyId == familyId)
            .ToListAsync(cancellationToken);
        if (sessions.Count == 0)
        {
            return Result.Failure(new DomainError(
                "session.not_found",
                "The session was not found."));
        }

        foreach (var session in sessions.Where(session => session.RevokedAt == null))
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }

        database.SecurityAuditRecords.Add(NewAudit(
            "identity.session.revoked",
            userId,
            sessions[0].OrganizationId,
            "refresh_family",
            familyId.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task RevokeOtherFamiliesAsync(
        Guid userId,
        Guid currentFamilyId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var sessions = await database.RefreshSessions
            .Where(session => session.UserId == userId
                && session.FamilyId != currentFamilyId
                && session.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }

        database.SecurityAuditRecords.Add(NewAudit(
            "identity.session.other_sessions_revoked",
            userId,
            null,
            "refresh_family",
            currentFamilyId.ToString("D")));
        await database.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsFamilyActiveAsync(
        Guid userId,
        Guid familyId,
        CancellationToken cancellationToken = default) =>
        database.RefreshSessions.AnyAsync(
            session => session.UserId == userId
                && session.FamilyId == familyId
                && session.UsedAt == null
                && session.RevokedAt == null
                && session.ExpiresAt > clock.UtcNow,
            cancellationToken);

    public async Task<IReadOnlyCollection<SessionSummaryDto>> ListForUserAsync(
        Guid userId,
        Guid? currentFamilyId,
        CancellationToken cancellationToken = default)
    {
        var rows = await database.RefreshSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId
                && session.RevokedAt == null
                && session.ExpiresAt > clock.UtcNow)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(cancellationToken);
        var summaries = rows
            .GroupBy(session => session.FamilyId)
            .Select(group => group.OrderByDescending(session => session.CreatedAt).First())
            .Select(session => new SessionSummaryDto(
                session.FamilyId,
                session.CreatedAt,
                session.ExpiresAt,
                session.LastSeenAt,
                session.AssuranceLevel,
                session.DeviceLabel,
                currentFamilyId == session.FamilyId))
            .OrderByDescending(session => session.LastSeenAt)
            .ToArray();
        return Array.AsReadOnly(summaries);
    }

    private async Task RevokeFamilyInternalAsync(
        Guid familyId,
        string reason,
        CancellationToken cancellationToken)
    {
        var sessions = await database.RefreshSessions
            .Where(session => session.FamilyId == familyId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = clock.UtcNow;
            session.RevokeReason = reason;
        }
    }

    private (RefreshSession Session, RefreshTokenDto Token) CreateSession(
        Guid userId,
        Guid? organizationId,
        Guid? membershipId,
        Guid familyId,
        Guid? parentSessionId,
        string assuranceLevel,
        string? deviceLabel)
    {
        var raw = CreateToken();
        var now = clock.UtcNow;
        var expiresAt = now.Add(RefreshLifetime);
        return (
            new RefreshSession
            {
                Id = Guid.NewGuid(),
                FamilyId = familyId,
                UserId = userId,
                OrganizationId = organizationId,
                MembershipId = membershipId,
                ParentSessionId = parentSessionId,
                TokenHash = Hash(raw),
                CreatedAt = now,
                ExpiresAt = expiresAt,
                LastSeenAt = now,
                AssuranceLevel = assuranceLevel,
                DeviceLabel = deviceLabel,
            },
            new RefreshTokenDto(raw, expiresAt, familyId));
    }

    private SecurityAuditRecord NewAudit(
        string eventType,
        Guid userId,
        Guid? organizationId,
        string targetType,
        string targetId) => new()
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            ActorUserId = userId,
            OrganizationId = organizationId,
            TargetType = targetType,
            TargetId = targetId,
            OccurredAt = clock.UtcNow,
            MetadataJson = "{}",
        };

    private static string NormalizeAssurance(string assuranceLevel) =>
        string.Equals(assuranceLevel, "mfa", StringComparison.Ordinal) ? "mfa" : "pwd";

    private static string? NormalizeDeviceLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 256 ? trimmed : trimmed[..256];
    }

    private static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string Hash(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static Result<RefreshRotationDto> Invalid() =>
        Result.Failure<RefreshRotationDto>(new DomainError(
            "refresh.invalid",
            "The refresh token is invalid or expired."));
}
