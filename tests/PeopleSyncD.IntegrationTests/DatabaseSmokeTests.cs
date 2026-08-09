using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PeopleSyncD.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PeopleSyncD.IntegrationTests;

public sealed class DatabaseSmokeTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("peoplesyncd")
        .WithUsername("peoplesyncd")
        .WithPassword("integration-test-password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task PostgreSql_is_reachable_and_context_can_connect()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("select 1", connection);
        var result = await command.ExecuteScalarAsync();

        Assert.Equal(1, result);

        var services = new ServiceCollection();
        services.AddDbContext<PeopleSyncDDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PeopleSyncDDbContext>();

        Assert.True(await db.Database.CanConnectAsync());
    }
}
