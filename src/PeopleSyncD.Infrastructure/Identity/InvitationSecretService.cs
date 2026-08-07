using System.Security.Cryptography;
using System.Text;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class InvitationSecretService : IInvitationSecretService
{
    public InvitationSecret Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return new InvitationSecret(token, Hash(token));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
