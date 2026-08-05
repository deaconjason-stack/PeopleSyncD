using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Lists organizations the authenticated user may select.
/// </summary>
public sealed class ListOrganizationsService(IOrganizationMembershipRepository memberships)
{
    public Task<IReadOnlyCollection<OrganizationAccessDto>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        memberships.ListForUserAsync(userId, cancellationToken);
}
