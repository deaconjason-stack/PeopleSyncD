namespace PeopleSyncD.Infrastructure.Persistence;

internal sealed class MfaTotpState
{
    public Guid UserId { get; set; }

    public long LastAcceptedCounter { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
