using PeopleSyncD.Domain.Organizations;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Persistence boundary for organization aggregates.
/// </summary>
public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
}
