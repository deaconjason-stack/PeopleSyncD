using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.ToTable("passkey_credentials");
        builder.HasKey(credential => credential.Id);
        builder.Property(credential => credential.CredentialId).HasMaxLength(1024).IsRequired();
        builder.Property(credential => credential.PublicKey).IsRequired();
        builder.Property(credential => credential.UserHandle).IsRequired();
        builder.Property(credential => credential.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(credential => credential.Transports).HasMaxLength(256);
        builder.HasIndex(credential => credential.CredentialId).IsUnique();
        builder.HasIndex(credential => new { credential.UserId, credential.RevokedAt });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(credential => credential.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
