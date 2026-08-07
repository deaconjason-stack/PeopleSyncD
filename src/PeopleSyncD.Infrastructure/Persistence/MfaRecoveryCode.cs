namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class MfaRecoveryCode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BatchId { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
