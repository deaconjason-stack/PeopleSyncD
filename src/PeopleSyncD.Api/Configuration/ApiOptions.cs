namespace PeopleSyncD.Api.Configuration;

/// <summary>
/// API presentation settings.
/// </summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string Name { get; init; } = "PeopleSyncD API";
}
