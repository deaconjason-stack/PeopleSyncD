using FluentValidation;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class InviteMemberService(
    IValidator<CreateInvitationRequest> validator,
    IOrganizationMembershipRepository memberships,
    IOrganizationInvitationRepository invitations,
    IOrganizationRepository organizations,
    IIdentityAdministrationGateway identities,
    IInvitationSecretService secrets,
    IIdentityNotificationSender notifications,
    IAuditRecorder audit,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<Result<InvitationDto>> ExecuteAsync(
        Guid actorUserId,
        Guid organizationId,
        CreateInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var actor = await memberships.GetActiveAsync(actorUserId, organizationId, cancellationToken);
        if (actor is null || actor.Role is not (TenantRole.Owner or TenantRole.Administrator))
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.forbidden",
                "Only an organization owner or administrator can invite members."));
        }

        if (request.Role is TenantRole.None or TenantRole.Owner)
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.role_invalid",
                "Owner access cannot be granted through an invitation."));
        }

        var organization = await organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null)
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.organization_missing",
                "The organization is unavailable."));
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await identities.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null
            && await memberships.GetAsync(existingUser.Id, organizationId, cancellationToken) is not null)
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.membership_conflict",
                "The user already has a membership in this organization."));
        }

        if (await invitations.HasPendingAsync(organizationId, email, cancellationToken))
        {
            return Result.Failure<InvitationDto>(new DomainError(
                "invitation.pending_conflict",
                "A pending invitation already exists for this email address."));
        }

        var secret = secrets.Create();
        var expiresAt = clock.UtcNow.AddDays(7);
        var creation = OrganizationInvitation.Create(
            organizationId,
            actorUserId,
            email,
            request.DisplayName,
            request.Role,
            secret.Hash,
            clock.UtcNow,
            expiresAt);
        if (creation.IsFailure)
        {
            return Result.Failure<InvitationDto>(creation.Error);
        }

        await invitations.AddAsync(creation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notifications.SendInvitationAsync(
            email,
            organization.Name,
            request.Role,
            secret.Token,
            expiresAt,
            cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "membership.invitation.created",
            actorUserId,
            organizationId,
            "invitation",
            creation.Value.Id.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string>
            {
                ["role"] = request.Role.ToString(),
                ["email_domain"] = email.Split('@').Last(),
            }), cancellationToken);

        return Result.Success(ToDto(creation.Value));
    }

    private static InvitationDto ToDto(OrganizationInvitation invitation) => new(
        invitation.Id,
        invitation.OrganizationId,
        invitation.Email,
        invitation.DisplayName,
        invitation.Role,
        invitation.Status,
        invitation.CreatedAt,
        invitation.ExpiresAt);
}
