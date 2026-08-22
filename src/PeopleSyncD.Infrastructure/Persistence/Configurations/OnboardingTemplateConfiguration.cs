using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Onboarding;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class OnboardingTemplateConfiguration : IEntityTypeConfiguration<OnboardingTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplate> builder)
    {
        builder.ToTable("onboarding_templates");
        builder.HasKey(onboardingTemplate => onboardingTemplate.Id);
        builder.Property(onboardingTemplate => onboardingTemplate.OrganizationId).IsRequired();
        builder.Property(onboardingTemplate => onboardingTemplate.Name).HasMaxLength(200).IsRequired();
        builder.Property(onboardingTemplate => onboardingTemplate.Version).IsRequired();
        builder.Property(onboardingTemplate => onboardingTemplate.IsActive).IsRequired();
        builder.HasIndex(onboardingTemplate => new { onboardingTemplate.OrganizationId, onboardingTemplate.Version }).IsUnique();
        builder.HasIndex(onboardingTemplate => new { onboardingTemplate.OrganizationId, onboardingTemplate.IsActive });
        builder.HasMany(onboardingTemplate => onboardingTemplate.Tasks)
            .WithOne()
            .HasForeignKey("TemplateId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(onboardingTemplate => onboardingTemplate.Tasks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(onboardingTemplate => onboardingTemplate.DomainEvents);
    }
}

internal sealed class OnboardingTemplateTaskConfiguration : IEntityTypeConfiguration<OnboardingTemplateTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplateTask> builder)
    {
        builder.ToTable("onboarding_template_tasks");
        builder.HasKey(task => task.Id);
        builder.Property<Guid>("TemplateId").IsRequired();
        builder.Property(task => task.Title).HasMaxLength(200).IsRequired();
        builder.Property(task => task.Category).HasMaxLength(80).IsRequired();
        builder.Property(task => task.Order).HasColumnName("SortOrder").IsRequired();
        builder.Property(task => task.DueOffsetDays).IsRequired();
        builder.HasIndex("TemplateId", nameof(OnboardingTemplateTask.Order)).IsUnique();
    }
}
