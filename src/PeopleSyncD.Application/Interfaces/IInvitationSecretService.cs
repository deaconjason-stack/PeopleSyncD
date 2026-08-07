using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Application.Interfaces;

public interface IInvitationSecretService
{
    InvitationSecret Create();

    string Hash(string token);
}
