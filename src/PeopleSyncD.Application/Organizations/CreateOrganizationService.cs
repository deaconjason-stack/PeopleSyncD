using FluentValidation;
using PeopleSyncD.Application.DTOs;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Organizations;

/// <summary>
/// Coordinates validated organization creation.
/// </summary>
public sealed class CreateOrganizationService(
    IOrganizationRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateOrganizationRequest> validator,
    IClock clock)
{
    public async Task<Result<OrganizationDto>> ExecuteAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OrganizationDto>.Failure(new Error(
                "organization.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        if (await repository.SlugExistsAsync(normalizedSlug, cancellationToken))
        {
            return Result<OrganizationDto>.Failure(new Error(
                "organization.slug_conflict",
                "An organization with this slug already exists."));
        }

        var creation = Organization.Create(request.Name, normalizedSlug, clock.UtcNow);
        if (creation.IsFailure)
        {
            return Result<OrganizationDto>.Failure(creation.Error);
        }

        await repository.AddAsync(creation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var organization = creation.Value;
        return Result<OrganizationDto>.Success(new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Slug,
            organization.CreatedAt));
    }
}
