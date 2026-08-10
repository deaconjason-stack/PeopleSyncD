using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class MfaChallengeConfiguration : IEntityTypeConfiguration<MfaChallenge>
{
    public void Configure(EntityTypeBuilder<MfaChallenge> builder)
    {
        builder.ToTable("mfa_challenges");
        builder.HasKey(challenge => challenge.Id);
        builder.Property(challenge => challenge.Purpose).HasMaxLength(32).IsRequired();
        builder.Property(challenge => challenge.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(challenge => challenge.TokenHash).IsUnique();
        builder.HasIndex(challenge => new { challenge.UserId, challenge.ExpiresAt });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(challenge => challenge.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
