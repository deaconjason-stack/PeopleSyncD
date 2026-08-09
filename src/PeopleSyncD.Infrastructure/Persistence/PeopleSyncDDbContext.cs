using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Domain.People;

namespace PeopleSyncD.Infrastructure.Persistence;

public sealed class PeopleSyncDDbContext(DbContextOptions<PeopleSyncDDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("peoplesyncd");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PeopleSyncDDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
