using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Identity;
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

    public Task<OrganizationMembership?> GetByIdAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        database.OrganizationMemberships.SingleOrDefaultAsync(
            membership => membership.Id == membershipId,
            cancellationToken);

    public Task<OrganizationMembership?> GetAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        database.OrganizationMemberships.SingleOrDefaultAsync(
            membership => membership.UserId == userId && membership.OrganizationId == organizationId,
            cancellationToken);

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken = default) =>
        await database.OrganizationMemberships.AddAsync(membership, cancellationToken);

    public Task<int> CountActiveOwnersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        database.OrganizationMemberships.CountAsync(
            membership => membership.OrganizationId == organizationId
                && membership.Role == TenantRole.Owner
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

    public async Task<IReadOnlyCollection<MembershipAdminDto>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var items = await (
            from membership in database.OrganizationMemberships.AsNoTracking()
            join user in database.Set<ApplicationUser>().AsNoTracking()
                on membership.UserId equals user.Id
            where membership.OrganizationId == organizationId
            orderby user.DisplayName, user.Email
            select new MembershipAdminDto(
                membership.Id,
                user.Id,
                membership.OrganizationId,
                user.DisplayName,
                user.Email ?? string.Empty,
                membership.Role,
                membership.Status,
                user.EmailConfirmed,
                user.TwoFactorEnabled))
            .ToListAsync(cancellationToken);
        return items.AsReadOnly();
    }
}
