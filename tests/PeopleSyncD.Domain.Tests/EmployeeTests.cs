using PeopleSyncD.Domain.Employees;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class EmployeeTests
{
    [Fact]
    public void CreateDefaultsToOnboardingAndPreservesTenant()
    {
        var tenantId = Guid.NewGuid();

        var result = Employee.Create(
            tenantId,
            "EFM-1001",
            "Jordan Carter",
            "jordan@example.test",
            "STEM Instructor",
            "Education",
            null,
            "St. Louis",
            EmploymentType.FullTime,
            new DateOnly(2026, 8, 24));

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.OrganizationId);
        Assert.Equal(EmploymentStatus.Onboarding, result.Value.Status);
    }

    [Fact]
    public void SeparatedEmployeeCannotBeReactivatedByGenericProfileUpdate()
    {
        var employee = CreateEmployee();

        Assert.True(employee.Activate().IsSuccess);
        Assert.True(employee.Separate(new DateOnly(2026, 8, 31)).IsSuccess);
        Assert.True(employee.UpdateProfile(
            "Jordan Carter",
            "jordan@example.test",
            "Manager",
            "Education",
            null,
            "St. Louis",
            EmploymentType.FullTime).IsSuccess);

        Assert.Equal(EmploymentStatus.Separated, employee.Status);
    }

    private static Employee CreateEmployee()
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            "EFM-1001",
            "Jordan Carter",
            "jordan@example.test",
            "STEM Instructor",
            "Education",
            null,
            "St. Louis",
            EmploymentType.FullTime,
            new DateOnly(2026, 8, 24));

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
