using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class PasskeyFoundationApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task RegistrationOptionsRequireResidentCredentialAndUserVerification()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = Bearer(account.AccessToken);

        var response = await client.PostAsync("/api/v1/auth/passkeys/registration/options", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var ceremony = await response.Content.ReadFromJsonAsync<PasskeyCeremonyOptionsDto>(JsonOptions);
        Assert.NotNull(ceremony);
        Assert.Equal("registration", ceremony.Purpose);
        using var document = JsonDocument.Parse(ceremony.PublicKeyOptionsJson);
        var selection = document.RootElement.GetProperty("authenticatorSelection");
        Assert.Equal("required", selection.GetProperty("residentKey").GetString());
        Assert.Equal("required", selection.GetProperty("userVerification").GetString());
    }

    [Fact]
    public async Task UnknownOrUnregisteredPasskeyLoginUsesGenericFailure()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/passkeys/authentication/options",
            new BeginPasskeyAuthenticationRequest("nobody@example.test"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("authentication.passkey_unavailable", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task StaleAuthenticationCannotBeginPasskeyRegistration()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var account = await RegisterAsync(client);
        using var scope = factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IAccessTokenIssuer>();
        var stale = issuer.Issue(
            account.User,
            authenticatedAt: DateTimeOffset.UtcNow.AddMinutes(-10),
            authenticationMethod: "pwd");
        client.DefaultRequestHeaders.Authorization = Bearer(stale.AccessToken);

        var response = await client.PostAsync("/api/v1/auth/passkeys/registration/options", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("authentication.reauthentication_required", problem.GetProperty("title").GetString());
    }

    private static async Task<AccessTokenDto> RegisterAsync(HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                "Passkey Foundation",
                $"passkey-{suffix}",
                "Passkey Owner",
                $"passkey-{suffix}@example.test",
                "ValidPassword!2026"),
            JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
    }

    private static AuthenticationHeaderValue Bearer(string token) => new("Bearer", token);

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));
}
