namespace PeopleSyncD.Api.Identity;

public sealed class AuthenticationConfiguration
{
    public const string SectionName = "Authentication";
    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public bool RequireHttpsMetadata { get; init; } = true;
}
