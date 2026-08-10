using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;
using Xunit;

namespace PeopleSyncD.Application.Tests;

public sealed class PrivilegedAuthenticationPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 7, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RecentAuthenticationIsAccepted()
    {
        var policy = new PrivilegedAuthenticationPolicy(new FixedClock(Now));

        var result = policy.Validate(Now.AddMinutes(-4));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RefreshedButOldAuthenticationIsRejected()
    {
        var policy = new PrivilegedAuthenticationPolicy(new FixedClock(Now));

        var result = policy.Validate(Now.AddMinutes(-6));

        Assert.True(result.IsFailure);
        Assert.Equal("authentication.reauthentication_required", result.Error.Code);
    }

    [Fact]
    public void MissingAuthenticationTimeIsRejected()
    {
        var policy = new PrivilegedAuthenticationPolicy(new FixedClock(Now));

        var result = policy.Validate(null);

        Assert.True(result.IsFailure);
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}
