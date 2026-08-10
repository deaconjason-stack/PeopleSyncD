namespace PeopleSyncD.Infrastructure.Configuration;

internal sealed class PasskeyOptions
{
    public const string SectionName = "WebAuthn";

    public string RelyingPartyId { get; init; } = "localhost";

    public string RelyingPartyName { get; init; } = "PeopleSyncD";

    public string[] Origins { get; init; } = ["http://localhost:3000"];

    public int CeremonyMinutes { get; init; } = 5;
}
