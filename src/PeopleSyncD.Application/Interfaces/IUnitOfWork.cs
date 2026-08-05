namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Commits one application transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
