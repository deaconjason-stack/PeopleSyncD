using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Persistence;
using Xunit;

namespace PeopleSyncD.Infrastructure.Tests;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task SaveChangesAsyncPersistsOrganization()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        var organization = Organization.Create(
            "PeopleSyncD",
            "peoplesyncd",
            DateTimeOffset.UtcNow).Value;

        await context.Organizations.AddAsync(organization);
        await context.SaveChangesAsync();

        var persisted = await context.Organizations.SingleAsync();
        Assert.Equal(organization.Id, persisted.Id);
    }

    [Fact]
    public async Task SaveChangesAsyncPersistsOrganizationMembership()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new ApplicationDbContext(options);
        var organization = Organization.Create(
            "PeopleSyncD",
            $"peoplesyncd-{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow).Value;
        var membership = OrganizationMembership.Create(
            Guid.NewGuid(),
            organization.Id,
            TenantRole.Owner,
            DateTimeOffset.UtcNow).Value;

        await context.Organizations.AddAsync(organization);
        await context.OrganizationMemberships.AddAsync(membership);
        await context.SaveChangesAsync();

        var persisted = await context.OrganizationMemberships.SingleAsync();
        Assert.Equal(TenantRole.Owner, persisted.Role);
        Assert.Equal(MembershipStatus.Active, persisted.Status);
    }
}
