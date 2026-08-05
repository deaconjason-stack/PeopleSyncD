using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Telemetry;

/// <summary>
/// Production UTC clock.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
