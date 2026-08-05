using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Organization membership query and persistence boundary.
/// </summary>
public interface IOrganizationMembershipRepository
{
    Task<OrganizationMembership?> GetActiveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrganizationAccessDto>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
