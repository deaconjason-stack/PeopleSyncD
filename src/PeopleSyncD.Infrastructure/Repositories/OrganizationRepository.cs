using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Repositories;

/// <summary>
/// EF Core organization repository.
/// </summary>
public sealed class OrganizationRepository(ApplicationDbContext dbContext) : IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Organizations.SingleOrDefaultAsync(organization => organization.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        dbContext.Organizations.AnyAsync(organization => organization.Slug == slug, cancellationToken);

    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default) =>
        await dbContext.Organizations.AddAsync(organization, cancellationToken);
}
