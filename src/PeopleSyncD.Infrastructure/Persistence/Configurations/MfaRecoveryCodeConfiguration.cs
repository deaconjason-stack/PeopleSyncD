using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class MfaRecoveryCodeConfiguration : IEntityTypeConfiguration<MfaRecoveryCode>
{
    public void Configure(EntityTypeBuilder<MfaRecoveryCode> builder)
    {
        builder.ToTable("mfa_recovery_codes");
        builder.HasKey(code => code.Id);
        builder.Property(code => code.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(code => code.CodeHash).IsUnique();
        builder.HasIndex(code => new { code.UserId, code.RevokedAt, code.UsedAt });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(code => code.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
