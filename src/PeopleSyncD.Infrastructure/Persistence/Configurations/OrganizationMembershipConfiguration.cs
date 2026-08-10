using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMembership>
{
    public void Configure(EntityTypeBuilder<OrganizationMembership> builder)
    {
        builder.ToTable("organization_memberships");
        builder.HasKey(membership => membership.Id);
        builder.Property(membership => membership.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(membership => membership.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(membership => new { membership.UserId, membership.OrganizationId }).IsUnique();
        builder.HasIndex(membership => new { membership.OrganizationId, membership.Status });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(membership => membership.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(membership => membership.DomainEvents);
    }
}
