namespace PeopleSyncD.SharedKernel;

/// <summary>
/// Marker contract for immutable domain events.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

/// <summary>
/// Base event carrying the occurrence timestamp.
/// </summary>
public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
