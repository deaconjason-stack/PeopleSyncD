using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PeopleSyncD.Application.DTOs;
using PeopleSyncD.Application.Organizations;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class ApiFoundationTests : IClassFixture<WebApplicationFactory<Program>>
{
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
    public async Task CreateOrganizationReturnsCreatedResource()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/organizations",
            new CreateOrganizationRequest("PeopleSyncD", $"peoplesyncd-{Guid.NewGuid():N}"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var organization = await response.Content.ReadFromJsonAsync<OrganizationDto>();
        Assert.NotNull(organization);
        Assert.Equal("PeopleSyncD", organization.Name);
    }
}
