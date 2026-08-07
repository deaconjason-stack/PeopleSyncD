using FluentValidation;

namespace PeopleSyncD.Application.Identity;

public sealed class InviteMemberValidator : AbstractValidator<CreateInvitationRequest>
{
    public InviteMemberValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Role).IsInEnum();
    }
}
