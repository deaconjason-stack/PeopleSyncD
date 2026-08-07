using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Infrastructure.Identity;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class MfaTotpStateConfiguration : IEntityTypeConfiguration<MfaTotpState>
{
    public void Configure(EntityTypeBuilder<MfaTotpState> builder)
    {
        builder.ToTable("mfa_totp_states");
        builder.HasKey(state => state.UserId);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(state => state.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
