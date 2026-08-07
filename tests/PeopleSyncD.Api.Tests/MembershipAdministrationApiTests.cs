using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Persistence;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class MembershipAdministrationApiTests
{
    [Fact]
    public async Task VerifiedOwnerCanCreateInvitation()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var selected = await RegisterVerifyAndSelectAsync(factory, client);
        var tenant = Assert.IsType<TenantContextDto>(selected.Tenant);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", selected.AccessToken);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/organizations/{tenant.OrganizationId:D}/invitations",
            new CreateInvitationRequest("invitee@example.com", "Invitee", TenantRole.Member));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var invitation = await response.Content.ReadFromJsonAsync<InvitationDto>();
        Assert.NotNull(invitation);
        Assert.Equal(TenantRole.Member, invitation.Role);
    }

    [Fact]
    public async Task SuspendedMembershipInvalidatesExistingTenantToken()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var selected = await RegisterVerifyAndSelectAsync(factory, client);
        var tenant = Assert.IsType<TenantContextDto>(selected.Tenant);

        using (var scope = factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var membership = await database.OrganizationMemberships.SingleAsync(item => item.Id == tenant.MembershipId);
            Assert.True(membership.Suspend(DateTimeOffset.UtcNow).IsSuccess);
            await database.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", selected.AccessToken);
        var response = await client.GetAsync($"/api/v1/organizations/{tenant.OrganizationId:D}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));

    private static async Task<AccessTokenDto> RegisterVerifyAndSelectAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                "M2 Test Organization",
                $"m2-{suffix}",
                "Test Owner",
                $"owner-{suffix}@example.com",
                "Correct-Horse-9!Battery"));
        registerResponse.EnsureSuccessStatusCode();
        var registration = (await registerResponse.Content.ReadFromJsonAsync<AccessTokenDto>())!;
        await ApiFoundationTests.ConfirmEmailAsync(factory, client, registration.User.Id);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registration.AccessToken);
        var selectResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/select-organization",
            new SelectOrganizationRequest(registration.Tenant!.OrganizationId));
        selectResponse.EnsureSuccessStatusCode();
        return (await selectResponse.Content.ReadFromJsonAsync<AccessTokenDto>())!;
    }
}
