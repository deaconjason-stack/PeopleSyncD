namespace PeopleSyncD.Infrastructure.Configuration;

/// <summary>
/// Database provider and migration behavior settings.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = "PostgreSql";

    public bool ApplyMigrationsOnStartup { get; init; }
}
