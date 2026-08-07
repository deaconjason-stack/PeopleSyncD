namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class PasskeyCredential
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CredentialId { get; set; } = string.Empty;

    public byte[] PublicKey { get; set; } = [];

    public byte[] UserHandle { get; set; } = [];

    public long SignatureCounter { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? Transports { get; set; }

    public bool BackupEligible { get; set; }

    public bool BackedUp { get; set; }

    public Guid AaGuid { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
