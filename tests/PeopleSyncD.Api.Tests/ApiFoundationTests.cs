using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Infrastructure.Identity;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class ApiFoundationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly WebApplicationFactory<Program> _factory;

    public ApiFoundationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Database:Provider", "InMemory"));
    }

    [Fact]
    public async Task AliveReturnsSuccessAndCorrelationId()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/alive");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task RegisterVerifyLoginSelectAndProtectTenantBoundaries()
    {
        using var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var request = new RegisterTenantRequest(
            "PeopleSyncD Test Organization",
            $"peoplesyncd-{suffix}",
            "Test Owner",
            $"owner-{suffix}@example.test",
            "ValidPassword!2026");

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register-tenant", request, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(registered);
        Assert.NotNull(registered.Tenant);
        Assert.NotNull(registered.RefreshToken);
        Assert.False(registered.User.EmailConfirmed);

        await ConfirmEmailAsync(_factory, client, registered.User.Id);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(request.Email, request.Password),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(login);
        Assert.Null(login.Tenant);
        Assert.NotNull(login.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var organizations = await client.GetFromJsonAsync<IReadOnlyCollection<OrganizationAccessDto>>(
            "/api/v1/auth/organizations",
            JsonOptions);
        var access = Assert.Single(organizations!);

        var selectResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/select-organization",
            new SelectOrganizationRequest(access.OrganizationId),
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, selectResponse.StatusCode);
        var selected = await selectResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions);
        Assert.NotNull(selected);
        Assert.NotNull(selected.Tenant);
        Assert.NotNull(selected.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", selected.AccessToken);
        var ownOrganization = await client.GetAsync($"/api/v1/organizations/{access.OrganizationId:D}");
        Assert.Equal(HttpStatusCode.OK, ownOrganization.StatusCode);

        var foreignOrganization = await client.GetAsync($"/api/v1/organizations/{Guid.NewGuid():D}");
        Assert.Equal(HttpStatusCode.Forbidden, foreignOrganization.StatusCode);
    }

    internal static async Task ConfirmEmailAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        Assert.NotNull(user);
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/email-verification/confirm",
            new ConfirmEmailRequest(userId, token),
            JsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
