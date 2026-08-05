using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Organizations;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(organization => organization.Id);
        builder.Property(organization => organization.Name).HasMaxLength(200).IsRequired();
        builder.Property(organization => organization.Slug).HasMaxLength(80).IsRequired();
        builder.HasIndex(organization => organization.Slug).IsUnique();
        builder.Ignore(organization => organization.DomainEvents);
    }
}
