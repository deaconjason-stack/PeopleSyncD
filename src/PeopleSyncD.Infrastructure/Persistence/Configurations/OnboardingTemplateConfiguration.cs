using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Onboarding;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class OnboardingTemplateConfiguration : IEntityTypeConfiguration<OnboardingTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplate> builder)
    {
        builder.ToTable("onboarding_templates");
        builder.HasKey(template => template.Id);
        builder.Property(template => template.OrganizationId).IsRequired();
        builder.Property(template => template.Name).HasMaxLength(200).IsRequired();
        builder.Property(template => template.Version).IsRequired();
        builder.Property(template => template.IsActive).IsRequired();
        builder.HasIndex(template => new { template.OrganizationId, template.Version }).IsUnique();
        builder.HasIndex(template => new { template.OrganizationId, template.IsActive });
        builder.HasMany(template => template.Tasks)
            .WithOne()
            .HasForeignKey("TemplateId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(template => template.Tasks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(template => template.DomainEvents);
    }
}

internal sealed class OnboardingTemplateTaskConfiguration : IEntityTypeConfiguration<OnboardingTemplateTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplateTask> builder)
    {
        builder.ToTable("onboarding_template_tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Title).HasMaxLength(200).IsRequired();
        builder.Property(task => task.Category).HasMaxLength(80).IsRequired();
        builder.Property(task => task.Order).HasColumnName("SortOrder").IsRequired();
        builder.Property(task => task.DueOffsetDays).IsRequired();
        builder.HasIndex("TemplateId", "SortOrder").IsUnique();
    }
}
