using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PeopleSyncD.IntegrationTests;

public sealed class ApiContractTests
{
    [Fact]
    public async Task Api_root_reports_platform_identity()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("PeopleSyncD Enterprise Platform", body);
        Assert.Contains("0.1.0-alpha", body);
    }

    [Fact]
    public async Task Version_endpoint_reports_foundation_release()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/version");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("foundation", body);
    }
}
