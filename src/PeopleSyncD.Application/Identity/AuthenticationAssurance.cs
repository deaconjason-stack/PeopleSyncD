namespace PeopleSyncD.Application.Identity;

public static class AuthenticationAssurance
{
    public const string Password = "pwd";
    public const string Mfa = "mfa";
    public const string PhishingResistant = "phishing_resistant";

    public static string Normalize(string? value) => value switch
    {
        PhishingResistant => PhishingResistant,
        Mfa => Mfa,
        _ => Password,
    };

    public static bool SatisfiesMfa(string? value) =>
        value is Mfa or PhishingResistant;

    public static bool SatisfiesPhishingResistant(string? value) =>
        value is PhishingResistant;

    public static string DefaultMethod(string? assuranceLevel) => Normalize(assuranceLevel) switch
    {
        PhishingResistant => "passkey",
        Mfa => "totp",
        _ => "pwd",
    };
}
