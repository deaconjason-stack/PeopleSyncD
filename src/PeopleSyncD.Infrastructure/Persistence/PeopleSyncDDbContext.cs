using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Domain.People;

namespace PeopleSyncD.Infrastructure.Persistence;

public sealed class PeopleSyncDDbContext(DbContextOptions<PeopleSyncDDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("peoplesyncd");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PeopleSyncDDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
