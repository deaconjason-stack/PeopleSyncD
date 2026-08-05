using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Repositories;

internal sealed class OrganizationMembershipRepository(ApplicationDbContext database)
    : IOrganizationMembershipRepository
{
    public Task<OrganizationMembership?> GetActiveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        database.OrganizationMemberships.SingleOrDefaultAsync(
            membership => membership.UserId == userId
                && membership.OrganizationId == organizationId
                && membership.Status == MembershipStatus.Active,
            cancellationToken);

    public async Task<IReadOnlyCollection<OrganizationAccessDto>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var items = await (
            from membership in database.OrganizationMemberships.AsNoTracking()
            join organization in database.Organizations.AsNoTracking()
                on membership.OrganizationId equals organization.Id
            where membership.UserId == userId
                && membership.Status != MembershipStatus.Revoked
            orderby organization.Name
            select new OrganizationAccessDto(
                membership.Id,
                organization.Id,
                organization.Name,
                organization.Slug,
                membership.Role,
                membership.Status))
            .ToListAsync(cancellationToken);
        return items.AsReadOnly();
    }
}
