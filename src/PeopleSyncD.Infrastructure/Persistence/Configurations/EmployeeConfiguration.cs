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
        builder.Property(employee => employee.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(employee => employee.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value).Value)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();
        builder.HasIndex(employee => new { employee.OrganizationId, employee.Email }).IsUnique();
        builder.Ignore(employee => employee.DomainEvents);
    }
}
