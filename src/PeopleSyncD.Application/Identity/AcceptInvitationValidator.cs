using FluentValidation;

namespace PeopleSyncD.Application.Identity;

public sealed class AcceptInvitationValidator : AbstractValidator<AcceptInvitationRequest>
{
    public AcceptInvitationValidator()
    {
        RuleFor(request => request.Token).NotEmpty().MaximumLength(512);
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(12).MaximumLength(256);
    }
}
