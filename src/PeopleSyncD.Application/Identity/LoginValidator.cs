using FluentValidation;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Validates credential authentication requests.
/// </summary>
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
