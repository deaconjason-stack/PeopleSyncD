using PeopleSyncD.Domain.Employees;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class EmployeeTests
{
    [Fact]
    public void CreateDefaultsToOnboardingAndPreservesTenant()
    {
        var tenantId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 8, 24);

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
            startDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.OrganizationId);
        Assert.Equal("EFM-1001", result.Value.EmployeeNumber);
        Assert.Equal("Jordan Carter", result.Value.DisplayName);
        Assert.Equal("jordan@example.test", result.Value.Email.Value);
        Assert.Equal("STEM Instructor", result.Value.Title);
        Assert.Equal("Education", result.Value.Department);
        Assert.Null(result.Value.ManagerEmployeeId);
        Assert.Equal("St. Louis", result.Value.Location);
        Assert.Equal(EmploymentType.FullTime, result.Value.EmploymentType);
        Assert.Equal(startDate, result.Value.StartDate);
        Assert.Equal(EmploymentStatus.Onboarding, result.Value.Status);
        Assert.Null(result.Value.SeparationDate);
    }

    [Fact]
    public void CreateRejectsMissingEmployeeNumber()
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            " ",
            "Jordan Carter",
            "jordan@example.test",
            "STEM Instructor",
            "Education",
            null,
            "St. Louis",
            EmploymentType.FullTime,
            new DateOnly(2026, 8, 24));

        Assert.True(result.IsFailure);
        Assert.Equal("employee.invalid", result.Error.Code);
    }

    [Fact]
    public void CreateRejectsMissingStartDate()
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
            default);

        Assert.True(result.IsFailure);
        Assert.Equal("employee.start_date_required", result.Error.Code);
    }

    [Fact]
    public void OnboardingEmployeeCanActivate()
    {
        var employee = CreateEmployee();

        var result = employee.Activate();

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.Active, employee.Status);
    }

    [Fact]
    public void ActiveEmployeeCanTakeAndReturnFromLeave()
    {
        var employee = CreateEmployee();
        Assert.True(employee.Activate().IsSuccess);

        Assert.True(employee.PlaceOnLeave().IsSuccess);
        Assert.Equal(EmploymentStatus.Leave, employee.Status);
        Assert.True(employee.ReturnFromLeave().IsSuccess);
        Assert.Equal(EmploymentStatus.Active, employee.Status);
    }

    [Fact]
    public void ActiveEmployeeCanBeSuspended()
    {
        var employee = CreateEmployee();
        Assert.True(employee.Activate().IsSuccess);

        var result = employee.Suspend();

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.Suspended, employee.Status);
    }

    [Fact]
    public void EmployeeCanBeSeparatedAndThenArchived()
    {
        var employee = CreateEmployee();
        Assert.True(employee.Activate().IsSuccess);
        var separationDate = new DateOnly(2026, 8, 31);

        Assert.True(employee.Separate(separationDate).IsSuccess);
        Assert.Equal(EmploymentStatus.Separated, employee.Status);
        Assert.Equal(separationDate, employee.SeparationDate);
        Assert.True(employee.Archive().IsSuccess);
        Assert.Equal(EmploymentStatus.Archived, employee.Status);
    }

    [Fact]
    public void InvalidLifecycleTransitionReturnsStableError()
    {
        var employee = CreateEmployee();

        var result = employee.ReturnFromLeave();

        Assert.True(result.IsFailure);
        Assert.Equal("employee.invalid_transition", result.Error.Code);
        Assert.Equal(EmploymentStatus.Onboarding, employee.Status);
    }

    [Fact]
    public void SeparationBeforeStartDateIsRejected()
    {
        var employee = CreateEmployee();

        var result = employee.Separate(new DateOnly(2026, 8, 23));

        Assert.True(result.IsFailure);
        Assert.Equal("employee.invalid_separation_date", result.Error.Code);
        Assert.Equal(EmploymentStatus.Onboarding, employee.Status);
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

        Assert.Equal("Manager", employee.Title);
        Assert.Equal(EmploymentStatus.Separated, employee.Status);
    }

    [Fact]
    public void EmployeeCannotBeOwnManager()
    {
        var employee = CreateEmployee();

        var result = employee.UpdateProfile(
            "Jordan Carter",
            "jordan@example.test",
            "Manager",
            "Education",
            employee.Id,
            "St. Louis",
            EmploymentType.FullTime);

        Assert.True(result.IsFailure);
        Assert.Equal("employee.manager_self_reference", result.Error.Code);
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
