using FluentValidation;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Creates the first owner identity and tenant organization atomically.
/// </summary>
public sealed class RegisterTenantService(
    IValidator<RegisterTenantRequest> validator,
    IOrganizationRepository organizations,
    IIdentityGateway identities,
    ITenantProvisioningGateway provisioning,
    SessionTokenService sessions,
    IClock clock)
{
    public async Task<Result<AccessTokenDto>> ExecuteAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "registration.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedSlug = request.OrganizationSlug.Trim().ToLowerInvariant();
        if (await identities.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "registration.email_conflict",
                "An account already exists for this email address."));
        }

        if (await organizations.SlugExistsAsync(normalizedSlug, cancellationToken))
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "registration.slug_conflict",
                "An organization already exists for this slug."));
        }

        var organizationResult = Organization.Create(request.OrganizationName, normalizedSlug, clock.UtcNow);
        if (organizationResult.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(organizationResult.Error);
        }

        var userId = Guid.NewGuid();
        var membershipResult = OrganizationMembership.Create(
            userId,
            organizationResult.Value.Id,
            TenantRole.Owner,
            clock.UtcNow);
        if (membershipResult.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(membershipResult.Error);
        }

        var provisioned = await provisioning.ProvisionAsync(
            userId,
            request.DisplayName.Trim(),
            normalizedEmail,
            request.Password,
            organizationResult.Value,
            membershipResult.Value,
            cancellationToken);
        if (provisioned.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(provisioned.Error);
        }

        return Result.Success(await sessions.IssueAsync(
            provisioned.Value.User,
            provisioned.Value.Access,
            cancellationToken: cancellationToken));
    }
}
