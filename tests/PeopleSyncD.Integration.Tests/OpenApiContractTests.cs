using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PeopleSyncD.Integration.Tests;

public sealed class OpenApiContractTests
{
    [Fact]
    public async Task OpenApiDocumentIsAvailable()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/v1/organizations", content, StringComparison.Ordinal);
    }
}
