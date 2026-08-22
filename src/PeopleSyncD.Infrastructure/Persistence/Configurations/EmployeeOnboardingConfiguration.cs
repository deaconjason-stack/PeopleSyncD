using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Onboarding;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeOnboardingConfiguration : IEntityTypeConfiguration<EmployeeOnboarding>
{
    public void Configure(EntityTypeBuilder<EmployeeOnboarding> builder)
    {
        builder.ToTable("employee_onboarding");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.OrganizationId).IsRequired();
        builder.Property(item => item.EmployeeId).IsRequired();
        builder.Property(item => item.TemplateId).IsRequired();
        builder.Property(item => item.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.TemplateVersion).IsRequired();
        builder.Property(item => item.StartDate).IsRequired();
        builder.HasIndex(item => new { item.OrganizationId, item.EmployeeId }).IsUnique();
        builder.HasIndex(item => new { item.OrganizationId, item.StartDate });
        builder.HasMany(item => item.Tasks)
            .WithOne()
            .HasForeignKey("EmployeeOnboardingId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.Tasks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(item => item.CompletedTaskCount);
        builder.Ignore(item => item.ProgressPercent);
        builder.Ignore(item => item.DomainEvents);
    }
}

internal sealed class OnboardingTaskConfiguration : IEntityTypeConfiguration<OnboardingTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTask> builder)
    {
        builder.ToTable("onboarding_tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Title).HasMaxLength(200).IsRequired();
        builder.Property(task => task.Category).HasMaxLength(80).IsRequired();
        builder.Property(task => task.Order).HasColumnName("SortOrder").IsRequired();
        builder.Property(task => task.DueDate).IsRequired();
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(task => task.CompletedAt);
        builder.Property(task => task.Note).HasMaxLength(1000);
        builder.HasIndex("EmployeeOnboardingId", "SortOrder").IsUnique();
        builder.HasIndex("EmployeeOnboardingId", "Status", "DueDate");
    }
}
