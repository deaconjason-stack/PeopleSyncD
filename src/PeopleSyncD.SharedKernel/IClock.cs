namespace PeopleSyncD.SharedKernel;

/// <summary>
/// Provides testable access to current UTC time.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
