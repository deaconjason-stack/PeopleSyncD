namespace PeopleSyncD.SharedKernel;

/// <summary>
/// Base type for aggregate roots that publish domain events.
/// </summary>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets domain events raised by the aggregate in the current unit of work.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event.
    /// </summary>
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears events after they have been persisted or dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
