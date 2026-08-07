using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.ToTable("refresh_sessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(session => session.RevokeReason).HasMaxLength(128);
        builder.Property(session => session.AssuranceLevel).HasMaxLength(16).IsRequired();
        builder.Property(session => session.DeviceLabel).HasMaxLength(256);
        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => session.ParentSessionId).IsUnique();
        builder.HasIndex(session => new { session.FamilyId, session.RevokedAt });
        builder.HasIndex(session => new { session.MembershipId, session.RevokedAt });
        builder.HasIndex(session => new { session.UserId, session.FamilyId, session.RevokedAt });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
