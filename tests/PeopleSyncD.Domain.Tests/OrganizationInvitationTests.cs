using PeopleSyncD.Domain.Identity;
using Xunit;

namespace PeopleSyncD.Domain.Tests;

public sealed class OrganizationInvitationTests
{
    [Fact]
    public void CreateRejectsOwnerRole()
    {
        var now = DateTimeOffset.UtcNow;
        var result = OrganizationInvitation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "person@example.com",
            "Person",
            TenantRole.Owner,
            "ABC123",
            now,
            now.AddDays(7));

        Assert.True(result.IsFailure);
        Assert.Equal("invitation.role_invalid", result.Error.Code);
    }

    [Fact]
    public void AcceptIsSingleUse()
    {
        var now = DateTimeOffset.UtcNow;
        var invitation = OrganizationInvitation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "person@example.com",
            "Person",
            TenantRole.Member,
            "ABC123",
            now,
            now.AddDays(7)).Value;

        Assert.True(invitation.Accept(now.AddMinutes(1)).IsSuccess);
        Assert.True(invitation.Accept(now.AddMinutes(2)).IsFailure);
    }
}
