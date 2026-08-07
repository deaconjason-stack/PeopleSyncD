using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class SecurityAuditRecordConfiguration : IEntityTypeConfiguration<SecurityAuditRecord>
{
    public void Configure(EntityTypeBuilder<SecurityAuditRecord> builder)
    {
        builder.ToTable("security_audit_records");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.EventType).HasMaxLength(128).IsRequired();
        builder.Property(record => record.TargetType).HasMaxLength(64).IsRequired();
        builder.Property(record => record.TargetId).HasMaxLength(128).IsRequired();
        builder.Property(record => record.MetadataJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(record => new { record.OrganizationId, record.OccurredAt });
        builder.HasIndex(record => new { record.ActorUserId, record.OccurredAt });
    }
}
