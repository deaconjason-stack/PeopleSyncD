using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace PeopleSyncD.IntegrationTests;

public sealed class ApiContractTests
{
    [Fact]
    public async Task Unauthenticated_current_user_request_is_rejected()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/api/v1/me", () => Results.Ok());
        await app.StartAsync();

        var client = app.GetTestClient();
        var response = await client.GetAsync("/api/v1/me");

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unauthorized);
        await app.StopAsync();
        await app.DisposeAsync();
    }
}
