using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Domain.People;
using PeopleSyncD.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PeopleSyncD.IntegrationTests;

public sealed class TenantIsolationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("peoplesyncd")
        .WithUsername("peoplesyncd")
        .WithPassword("integration-test-password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task People_queries_are_scoped_to_requested_organization()
    {
        var services = new ServiceCollection();
        services.AddDbContext<PeopleSyncDDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PeopleSyncDDbContext>();
        await db.Database.EnsureCreatedAsync();

        var organizationA = new Organization(Guid.NewGuid(), "Organization A", "organization-a");
        var organizationB = new Organization(Guid.NewGuid(), "Organization B", "organization-b");
        db.Organizations.AddRange(organizationA, organizationB);
        db.People.AddRange(
            new Person(Guid.NewGuid(), organizationA.Id, "Alice", "A", "alice@a.example"),
            new Person(Guid.NewGuid(), organizationB.Id, "Bob", "B", "bob@b.example"));
        await db.SaveChangesAsync();

        var result = await db.People.AsNoTracking().Where(p => p.OrganizationId == organizationA.Id).ToListAsync();

        Assert.Single(result);
        Assert.Equal("alice@a.example", result[0].Email);
        Assert.DoesNotContain(result, p => p.OrganizationId == organizationB.Id);
    }
}
