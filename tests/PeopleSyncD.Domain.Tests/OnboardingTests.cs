using PeopleSyncD.Domain.Onboarding;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class OnboardingTests
{
    [Fact]
    public void StandardTemplateHasStableVersionAndSevenOrderedTasks()
    {
        var organizationId = Guid.NewGuid();

        var result = OnboardingTemplate.CreateStandard(organizationId, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(organizationId, result.Value.OrganizationId);
        Assert.Equal("Standard Employee Onboarding", result.Value.Name);
        Assert.Equal(1, result.Value.Version);
        Assert.True(result.Value.IsActive);
        Assert.Equal(7, result.Value.Tasks.Count);
        Assert.Equal(
            new[]
            {
                "Employment Paperwork",
                "Orientation",
                "Policy Acknowledgement",
                "Required Credentials",
                "Required Training",
                "Equipment/Access",
                "Manager Introduction",
            },
            result.Value.Tasks.OrderBy(task => task.Order).Select(task => task.Title));
    }

    [Fact]
    public void InstanceKeepsTemplateVersionAndTracksProgress()
    {
        var template = OnboardingTemplate.CreateStandard(Guid.NewGuid(), 1).Value;
        var onboarding = EmployeeOnboarding.Instantiate(
            template,
            Guid.NewGuid(),
            new DateOnly(2026, 8, 24)).Value;

        Assert.Equal(1, onboarding.TemplateVersion);
        Assert.Equal(7, onboarding.Tasks.Count);
        Assert.Equal(0, onboarding.CompletedTaskCount);
        Assert.Equal(0, onboarding.ProgressPercent);

        var first = onboarding.Tasks.OrderBy(task => task.Order).First();
        var result = onboarding.CompleteTask(
            first.Id,
            new DateTimeOffset(2026, 8, 24, 15, 0, 0, TimeSpan.Zero),
            "Completed during orientation.");

        Assert.True(result.IsSuccess);
        Assert.Equal(OnboardingTaskStatus.Completed, first.Status);
        Assert.Equal(1, onboarding.CompletedTaskCount);
        Assert.Equal(14, onboarding.ProgressPercent);
    }
}
