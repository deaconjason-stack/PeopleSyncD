using PeopleSyncD.Application.Employees;

namespace PeopleSyncD.Application.Hr;

public sealed record HrDashboardDto(
    int TotalEmployees,
    int ActiveEmployees,
    int OnboardingEmployees,
    int EmployeesOnLeave,
    int CredentialsExpiringSoon,
    int OverdueOnboardingTasks,
    int OpenHrCases,
    IReadOnlyCollection<EmployeeDto> RecentlyChangedEmployees);
