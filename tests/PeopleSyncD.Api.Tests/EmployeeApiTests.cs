using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Persistence;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class EmployeeApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task OwnerCanCreateListAndReadEmployee()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var tenant = await RegisterVerifyAndSelectAsync(factory, client, "owner-a");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenant.Selected.AccessToken);

        var create = await client.PostAsJsonAsync(
            "/api/v1/employees",
            EmployeePayload("EFM-1001", "Jordan Carter", "jordan@example.test"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var employeeId = created.GetProperty("id").GetGuid();

        var list = await client.GetAsync("/api/v1/employees?search=Jordan&status=Onboarding");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listed = await list.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Single(listed.EnumerateArray());

        var read = await client.GetAsync($"/api/v1/employees/{employeeId:D}");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var employee = await read.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("EFM-1001", employee.GetProperty("employeeNumber").GetString());
        Assert.Equal("Jordan Carter", employee.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task MemberWithoutEmployeeWritePermissionCannotCreateEmployee()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var tenant = await RegisterVerifyAndSelectAsync(factory, client, "member-test");
        var context = Assert.IsType<TenantContextDto>(tenant.Selected.Tenant);

        using (var scope = factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var membership = await database.OrganizationMemberships.SingleAsync(item => item.Id == context.MembershipId);
            Assert.True(membership.ChangeRole(TenantRole.Member, DateTimeOffset.UtcNow).IsSuccess);
            await database.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(tenant.Email, tenant.Password),
            JsonOptions);
        loginResponse.EnsureSuccessStatusCode();
        var login = (await loginResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var selectResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/select-organization",
            new SelectOrganizationRequest(context.OrganizationId),
            JsonOptions);
        selectResponse.EnsureSuccessStatusCode();
        var memberToken = (await selectResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken.AccessToken);
        var response = await client.PostAsJsonAsync(
            "/api/v1/employees",
            EmployeePayload("EFM-1002", "Taylor Morgan", "taylor@example.test"),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantCannotReadEmployeeOwnedByAnotherTenant()
    {
        await using var factory = CreateFactory();
        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();
        var tenantA = await RegisterVerifyAndSelectAsync(factory, clientA, "tenant-a");
        var tenantB = await RegisterVerifyAndSelectAsync(factory, clientB, "tenant-b");

        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenantA.Selected.AccessToken);
        var create = await clientA.PostAsJsonAsync(
            "/api/v1/employees",
            EmployeePayload("EFM-2001", "Morgan Reed", "morgan@example.test"),
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var employeeId = created.GetProperty("id").GetGuid();

        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tenantB.Selected.AccessToken);
        var response = await clientB.GetAsync($"/api/v1/employees/{employeeId:D}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static object EmployeePayload(string employeeNumber, string displayName, string email) => new
    {
        employeeNumber,
        displayName,
        email,
        title = "STEM Instructor",
        department = "Education",
        managerEmployeeId = (Guid?)null,
        location = "St. Louis",
        employmentType = EmploymentType.FullTime,
        startDate = new DateOnly(2026, 8, 24),
    };

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));

    private static async Task<TestTenant> RegisterVerifyAndSelectAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"{prefix}-{suffix}@example.test";
        const string password = "Correct-Horse-9!Battery";
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register-tenant",
            new RegisterTenantRequest(
                $"{prefix} organization",
                $"{prefix}-{suffix}",
                "Test Owner",
                email,
                password),
            JsonOptions);
        registerResponse.EnsureSuccessStatusCode();
        var registration = (await registerResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
        await ApiFoundationTests.ConfirmEmailAsync(factory, client, registration.User.Id);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registration.AccessToken);
        var selectResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/select-organization",
            new SelectOrganizationRequest(registration.Tenant!.OrganizationId),
            JsonOptions);
        selectResponse.EnsureSuccessStatusCode();
        var selected = (await selectResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
        return new TestTenant(selected, email, password);
    }

    private sealed record TestTenant(AccessTokenDto Selected, string Email, string Password);
}
