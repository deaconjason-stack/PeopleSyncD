using FluentValidation;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class AcceptInvitationService(
    IValidator<AcceptInvitationRequest> validator,
    IInvitationSecretService secrets,
    IOrganizationInvitationRepository invitations,
    IOrganizationMembershipRepository memberships,
    IOrganizationRepository organizations,
    IIdentityAdministrationGateway identities,
    IAuditRecorder audit,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<OrganizationAccessDto>> ExecuteAsync(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<OrganizationAccessDto>(new DomainError(
                "invitation.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var invitation = await invitations.GetByTokenHashAsync(secrets.Hash(request.Token), cancellationToken);
        if (invitation is null || invitation.Status != InvitationStatus.Pending || invitation.ExpiresAt < clock.UtcNow)
        {
            return Result.Failure<OrganizationAccessDto>(new DomainError(
                "invitation.invalid",
                "The invitation is invalid or expired."));
        }

        var organization = await organizations.GetByIdAsync(invitation.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result.Failure<OrganizationAccessDto>(new DomainError(
                "invitation.organization_missing",
                "The organization is unavailable."));
        }

        var identity = await identities.GetByEmailAsync(invitation.Email, cancellationToken);
        if (identity is null)
        {
            var created = await identities.CreateInvitedUserAsync(
                invitation.Email,
                string.IsNullOrWhiteSpace(request.DisplayName) ? invitation.DisplayName : request.DisplayName,
                request.Password,
                cancellationToken);
            if (created.IsFailure)
            {
                return Result.Failure<OrganizationAccessDto>(created.Error);
            }

            identity = created.Value;
        }

        if (await memberships.GetAsync(identity.Id, invitation.OrganizationId, cancellationToken) is not null)
        {
            return Result.Failure<OrganizationAccessDto>(new DomainError(
                "invitation.membership_conflict",
                "A membership already exists for this user."));
        }

        var membershipCreation = OrganizationMembership.Create(
            identity.Id,
            invitation.OrganizationId,
            invitation.Role,
            clock.UtcNow);
        if (membershipCreation.IsFailure)
        {
            return Result.Failure<OrganizationAccessDto>(membershipCreation.Error);
        }

        var acceptance = invitation.Accept(clock.UtcNow);
        if (acceptance.IsFailure)
        {
            return Result.Failure<OrganizationAccessDto>(acceptance.Error);
        }

        var emailConfirmation = await identities.ConfirmEmailFromInvitationAsync(identity.Id, cancellationToken);
        if (emailConfirmation.IsFailure)
        {
            return Result.Failure<OrganizationAccessDto>(emailConfirmation.Error);
        }

        await memberships.AddAsync(membershipCreation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "membership.invitation.accepted",
            identity.Id,
            invitation.OrganizationId,
            "membership",
            membershipCreation.Value.Id.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string> { ["role"] = invitation.Role.ToString() }), cancellationToken);

        return Result.Success(new OrganizationAccessDto(
            membershipCreation.Value.Id,
            invitation.OrganizationId,
            organization.Name,
            organization.Slug,
            invitation.Role,
            MembershipStatus.Active));
    }
}
