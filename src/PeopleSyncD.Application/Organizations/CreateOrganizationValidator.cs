using FluentValidation;

namespace PeopleSyncD.Application.Organizations;

/// <summary>
/// Validates organization creation input before domain execution.
/// </summary>
public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Slug)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain lowercase letters, numbers, and single hyphens only.");
    }
}
