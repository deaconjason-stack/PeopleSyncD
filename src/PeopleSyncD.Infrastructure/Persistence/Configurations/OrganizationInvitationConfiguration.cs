using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.ToTable("organization_invitations");
        builder.HasKey(invitation => invitation.Id);
        builder.Property(invitation => invitation.Email).HasMaxLength(320).IsRequired();
        builder.Property(invitation => invitation.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(invitation => invitation.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(invitation => invitation.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(invitation => invitation.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(invitation => invitation.TokenHash).IsUnique();
        builder.HasIndex(invitation => new { invitation.OrganizationId, invitation.Email, invitation.Status });
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(invitation => invitation.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(invitation => invitation.DomainEvents);
    }
}
