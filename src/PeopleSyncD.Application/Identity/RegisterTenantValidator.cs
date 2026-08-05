using FluentValidation;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Validates owner and organization bootstrap requests.
/// </summary>
public sealed class RegisterTenantValidator : AbstractValidator<RegisterTenantRequest>
{
    public RegisterTenantValidator()
    {
        RuleFor(request => request.OrganizationName)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(request => request.OrganizationSlug)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
    }
}
