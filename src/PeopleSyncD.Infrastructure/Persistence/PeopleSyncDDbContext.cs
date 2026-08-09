using Microsoft.EntityFrameworkCore;

namespace PeopleSyncD.Infrastructure.Persistence;

public sealed class PeopleSyncDDbContext(DbContextOptions<PeopleSyncDDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("peoplesyncd");
        base.OnModelCreating(modelBuilder);
    }
}
