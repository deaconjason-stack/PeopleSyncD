using PeopleSyncD.Domain.Organizations;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class OrganizationTests
{
    [Fact]
    public void CreateWithValidInputRaisesCreatedEvent()
    {
        var now = new DateTimeOffset(2026, 8, 5, 0, 0, 0, TimeSpan.Zero);

        var result = Organization.Create("MediSyncD Technologies", "medisyncd", now);

        Assert.True(result.IsSuccess);
        Assert.Equal("MediSyncD Technologies", result.Value.Name);
        Assert.Equal("medisyncd", result.Value.Slug);
        Assert.Single(result.Value.DomainEvents);
    }

    [Fact]
    public void CreateWithMissingNameReturnsFailure()
    {
        var result = Organization.Create(" ", "medisyncd", DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("organization.invalid", result.Error.Code);
    }
}
