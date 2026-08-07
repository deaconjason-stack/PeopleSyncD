using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Infrastructure.Identity;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class SessionSecurityApiTests
{
    [Fact]
    public async Task RefreshTokenRotatesAndReuseRevokesFamily()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var registrationResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                "Refresh Test",
                $"refresh-{suffix}",
                "Refresh Owner",
                $"refresh-{suffix}@example.com",
                "Correct-Horse-9!Battery"));
        registrationResponse.EnsureSuccessStatusCode();
        var registration = (await registrationResponse.Content.ReadFromJsonAsync<AccessTokenDto>())!;
        Assert.NotNull(registration.RefreshToken);
        await ApiFoundationTests.ConfirmEmailAsync(factory, client, registration.User.Id);

        var firstRotation = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest(registration.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, firstRotation.StatusCode);
        var rotated = await firstRotation.Content.ReadFromJsonAsync<AccessTokenDto>();
        Assert.NotNull(rotated);
        Assert.NotEqual(registration.RefreshToken, rotated.RefreshToken);

        var reuse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest(registration.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        var replacementAfterReuse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest(rotated.RefreshToken!));
        Assert.Equal(HttpStatusCode.Unauthorized, replacementAfterReuse.StatusCode);
    }

    [Fact]
    public async Task MfaEnabledAccountCannotBypassSecondFactorWithPasswordOnly()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"mfa-{suffix}@example.com";
        var password = "Correct-Horse-9!Battery";
        var registrationResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest("MFA Test", $"mfa-{suffix}", "MFA Owner", email, password));
        registrationResponse.EnsureSuccessStatusCode();

        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.TwoFactorEnabled = true;
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));
}
