using Microsoft.Extensions.DependencyInjection;

namespace PeopleSyncD.Infrastructure.Persistence;

/// <summary>
/// Development and test database initialization helpers.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeDevelopmentDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await database.Database.EnsureCreatedAsync(cancellationToken);
    }
}
