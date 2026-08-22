using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Employees;
using Xunit;

namespace PeopleSyncD.Api.Tests;

public sealed class OnboardingDashboardApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task OwnerCanOpenAndCompleteStandardOnboarding()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var token = await RegisterVerifyAndSelectAsync(factory, client, "onboarding-owner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var employeeId = await CreateEmployeeAsync(client, "EFM-3001", "Avery Brooks", "avery@example.test");

        var read = await client.GetAsync($"/api/v1/employees/{employeeId:D}/onboarding");

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var onboarding = await read.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(1, onboarding.GetProperty("templateVersion").GetInt32());
        Assert.Equal(7, onboarding.GetProperty("tasks").GetArrayLength());
        Assert.Equal(0, onboarding.GetProperty("progressPercent").GetInt32());
        var firstTaskId = onboarding.GetProperty("tasks")[0].GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync(
            $"/api/v1/employees/{employeeId:D}/onboarding/tasks/{firstTaskId:D}",
            new { status = "Completed", note = "Completed during demo orientation." },
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(14, updated.GetProperty("progressPercent").GetInt32());
        Assert.Equal("Completed", updated.GetProperty("tasks")[0].GetProperty("status").GetString());
    }

    [Fact]
    public async Task DashboardReflectsPersistedEmployeeCounts()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var token = await RegisterVerifyAndSelectAsync(factory, client, "dashboard-owner");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        await CreateEmployeeAsync(client, "EFM-3101", "Casey Lane", "casey@example.test");
        await CreateEmployeeAsync(client, "EFM-3102", "Riley Stone", "riley@example.test");

        var response = await client.GetAsync("/api/v1/hr/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal(2, dashboard.GetProperty("totalEmployees").GetInt32());
        Assert.Equal(2, dashboard.GetProperty("onboardingEmployees").GetInt32());
        Assert.Equal(0, dashboard.GetProperty("activeEmployees").GetInt32());
        Assert.Equal(0, dashboard.GetProperty("employeesOnLeave").GetInt32());
    }

    private static async Task<Guid> CreateEmployeeAsync(
        HttpClient client,
        string employeeNumber,
        string displayName,
        string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new
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
            },
            JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var employee = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return employee.GetProperty("id").GetGuid();
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("Database:Provider", "InMemory"));

    private static async Task<AccessTokenDto> RegisterVerifyAndSelectAsync(
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
        return (await selectResponse.Content.ReadFromJsonAsync<AccessTokenDto>(JsonOptions))!;
    }
}
