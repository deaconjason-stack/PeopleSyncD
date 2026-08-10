using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace PeopleSyncD.Infrastructure.Configuration;

/// <summary>
/// Validated JWT issuer configuration and process-local signing material.
/// </summary>
public sealed record JwtOptions(
    string Issuer,
    string Audience,
    string SigningKey,
    int AccessTokenMinutes)
{
    public const string SectionName = "Jwt";

    public static JwtOptions Create(
        IConfiguration configuration,
        bool allowEphemeralSigningKey)
    {
        var issuer = configuration[$"{SectionName}:Issuer"]?.Trim();
        var audience = configuration[$"{SectionName}:Audience"]?.Trim();
        var signingKey = configuration[$"{SectionName}:SigningKey"]?.Trim();
        var accessTokenText = configuration[$"{SectionName}:AccessTokenMinutes"];

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            if (!allowEphemeralSigningKey)
            {
                throw new InvalidOperationException(
                    "Jwt:SigningKey must be supplied by a protected configuration provider outside development.");
            }

            signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        if (signingKey.Length < 43)
        {
            throw new InvalidOperationException("Jwt:SigningKey must provide at least 256 bits of entropy.");
        }

        var accessTokenMinutes = 15;
        if (!string.IsNullOrWhiteSpace(accessTokenText)
            && int.TryParse(
                accessTokenText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsedMinutes))
        {
            accessTokenMinutes = parsedMinutes;
        }

        if (accessTokenMinutes is < 5 or > 60)
        {
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be between 5 and 60.");
        }

        return new JwtOptions(
            string.IsNullOrWhiteSpace(issuer) ? "PeopleSyncD" : issuer,
            string.IsNullOrWhiteSpace(audience) ? "PeopleSyncD.Clients" : audience,
            signingKey,
            accessTokenMinutes);
    }
}
