using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Application.Identity;

public sealed class ListMembersService(IOrganizationMembershipRepository memberships)
{
    public Task<IReadOnlyCollection<MembershipAdminDto>> ExecuteAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        memberships.ListForOrganizationAsync(organizationId, cancellationToken);
}
