using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class PasskeyCeremonyConfiguration : IEntityTypeConfiguration<PasskeyCeremony>
{
    public void Configure(EntityTypeBuilder<PasskeyCeremony> builder)
    {
        builder.ToTable("passkey_ceremonies");
        builder.HasKey(ceremony => ceremony.Id);
        builder.Property(ceremony => ceremony.Purpose).HasMaxLength(32).IsRequired();
        builder.Property(ceremony => ceremony.OptionsJson).IsRequired();
        builder.HasIndex(ceremony => new { ceremony.UserId, ceremony.Purpose, ceremony.ExpiresAt });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(ceremony => ceremony.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
