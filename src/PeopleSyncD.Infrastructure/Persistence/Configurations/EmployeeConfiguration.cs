using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.Domain.ValueObjects;

namespace PeopleSyncD.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.OrganizationId).IsRequired();
        builder.Property(employee => employee.EmployeeNumber).HasMaxLength(64).IsRequired();
        builder.Property(employee => employee.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(employee => employee.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();
        builder.Property(employee => employee.Title).HasMaxLength(200).IsRequired();
        builder.Property(employee => employee.Department).HasMaxLength(200).IsRequired();
        builder.Property(employee => employee.ManagerEmployeeId);
        builder.Property(employee => employee.Location).HasMaxLength(200).IsRequired();
        builder.Property(employee => employee.EmploymentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(employee => employee.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(employee => employee.StartDate).IsRequired();
        builder.Property(employee => employee.SeparationDate);
        builder.HasIndex(employee => new { employee.OrganizationId, employee.EmployeeNumber }).IsUnique();
        builder.HasIndex(employee => new { employee.OrganizationId, employee.Email }).IsUnique();
        builder.HasIndex(employee => new { employee.OrganizationId, employee.Status });
        builder.HasIndex(employee => new { employee.OrganizationId, employee.ManagerEmployeeId });
        builder.Ignore(employee => employee.DomainEvents);
    }
}
