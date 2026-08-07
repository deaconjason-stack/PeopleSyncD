using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class DevelopmentFileIdentityNotificationSender(IConfiguration configuration)
    : IIdentityNotificationSender
{
    public Task SendInvitationAsync(
        string email,
        string organizationName,
        TenantRole role,
        string acceptanceToken,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default) =>
        WriteAsync(
            "invitation",
            email,
            new
            {
                organizationName,
                role = role.ToString(),
                acceptanceToken,
                expiresAt,
            },
            cancellationToken);

    public Task SendEmailVerificationAsync(
        string email,
        Guid userId,
        string verificationToken,
        CancellationToken cancellationToken = default) =>
        WriteAsync(
            "email-verification",
            email,
            new { userId, verificationToken },
            cancellationToken);

    private async Task WriteAsync(
        string kind,
        string email,
        object payload,
        CancellationToken cancellationToken)
    {
        var environmentName = configuration["DOTNET_ENVIRONMENT"]
            ?? configuration["ASPNETCORE_ENVIRONMENT"];
        if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A production identity-notification transport must be configured before sending security tokens.");
        }

        var directory = Path.Combine(Directory.GetCurrentDirectory(), ".local-email");
        Directory.CreateDirectory(directory);
        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{kind}.json";
        var envelope = JsonSerializer.Serialize(new { kind, to = email, payload });
        await File.WriteAllTextAsync(Path.Combine(directory, fileName), envelope, cancellationToken);
    }
}
