using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Atomic persistence boundary for creating an owner, organization, and membership.
/// </summary>
public interface ITenantProvisioningGateway
{
    Task<Result<ProvisionedTenantDto>> ProvisionAsync(
        Guid userId,
        string displayName,
        string email,
        string password,
        Organization organization,
        OrganizationMembership membership,
        CancellationToken cancellationToken = default);
}
