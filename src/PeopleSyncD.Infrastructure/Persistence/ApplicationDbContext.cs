using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work for platform aggregates and identity records.
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();

    internal DbSet<SecurityAuditRecord> SecurityAuditRecords => Set<SecurityAuditRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
