using PeopleSyncD.Domain.Identity;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class OrganizationMembershipTests
{
    [Fact]
    public void CreateOwnerMembershipRaisesDomainEvent()
    {
        var result = OrganizationMembership.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TenantRole.Owner,
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(TenantRole.Owner, result.Value.Role);
        Assert.Equal(MembershipStatus.Active, result.Value.Status);
        Assert.Single(result.Value.DomainEvents);
    }

    [Fact]
    public void SuspendedMembershipCannotChangeRole()
    {
        var membership = OrganizationMembership.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TenantRole.Member,
            DateTimeOffset.UtcNow).Value;
        membership.Suspend(DateTimeOffset.UtcNow);

        var result = membership.ChangeRole(TenantRole.Manager, DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("membership.inactive", result.Error.Code);
    }
}
