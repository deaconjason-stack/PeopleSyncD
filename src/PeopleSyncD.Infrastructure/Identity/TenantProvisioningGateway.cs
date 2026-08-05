using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class TenantProvisioningGateway(
    ApplicationDbContext database,
    UserManager<ApplicationUser> users) : ITenantProvisioningGateway
{
    public async Task<Result<ProvisionedTenantDto>> ProvisionAsync(
        Guid userId,
        string displayName,
        string email,
        string password,
        Organization organization,
        OrganizationMembership membership,
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction? transaction = null;
        if (database.Database.IsRelational())
        {
            transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = email,
                Email = email,
                DisplayName = displayName,
                IsActive = true,
            };
            var identityResult = await users.CreateAsync(user, password);
            if (!identityResult.Succeeded)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                var description = string.Join(
                    " ",
                    identityResult.Errors.Select(error => error.Description));
                return Result.Failure<ProvisionedTenantDto>(new DomainError(
                    "registration.identity_rejected",
                    description));
            }

            await database.Organizations.AddAsync(organization, cancellationToken);
            await database.OrganizationMemberships.AddAsync(membership, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return Result.Success(new ProvisionedTenantDto(
                new IdentityUserDto(
                    user.Id,
                    user.DisplayName,
                    user.Email ?? string.Empty,
                    user.EmailConfirmed,
                    user.IsActive),
                new OrganizationAccessDto(
                    membership.Id,
                    organization.Id,
                    organization.Name,
                    organization.Slug,
                    membership.Role,
                    membership.Status)));
        }
        catch (DbUpdateException)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            return Result.Failure<ProvisionedTenantDto>(new DomainError(
                "registration.persistence_conflict",
                "The account or organization conflicts with an existing record."));
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
