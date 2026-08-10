using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using PeopleSyncD.Application.Identity;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class SessionAuthenticationFreshnessTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task RefreshRotationPreservesOriginalAuthenticationTimeAndMethod()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                "Freshness Test",
                $"freshness-{suffix}",
                "Freshness Owner",
                $"freshness-{suffix}@example.com",
                "Correct-Horse-9!Battery"),
            JsonOptions);
        response.EnsureSuccessStatusCode();
        var first = (await response.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
        Assert.NotNull(first.RefreshToken);
        await ApiFoundationTests.ConfirmEmailAsync(factory, client, first.User.Id);
        var firstClaims = ReadClaims(first.AccessToken);

        var rotation = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest(first.RefreshToken),
            JsonOptions);
        rotation.EnsureSuccessStatusCode();
        var second = (await rotation.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
        var secondClaims = ReadClaims(second.AccessToken);

        Assert.Equal(firstClaims.AuthTime, secondClaims.AuthTime);
        Assert.Equal("pwd", firstClaims.AuthenticationMethod);
        Assert.Equal(firstClaims.AuthenticationMethod, secondClaims.AuthenticationMethod);
        Assert.Equal(first.SessionFamilyId, second.SessionFamilyId);
    }

    private static (string AuthTime, string AuthenticationMethod) ReadClaims(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var authTime = jwt.Claims.Single(claim => claim.Type == "auth_time").Value;
        var methods = jwt.Claims.Where(claim => claim.Type == "amr").Select(claim => claim.Value).ToArray();
        return (authTime, methods.Contains("passkey", StringComparer.Ordinal) ? "passkey" : "pwd");
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));
}
