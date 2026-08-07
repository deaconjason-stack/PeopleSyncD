using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Permissions;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId:guid}")]
public sealed class OrganizationMembershipsController(
    ListMembersService listMembers,
    InviteMemberService inviteMember,
    UpdateMembershipService updateMembership,
    IOrganizationInvitationRepository invitations) : ControllerBase
{
    [Authorize(Policy = PermissionNames.MembershipsRead)]
    [HttpGet("members")]
    public async Task<ActionResult<IReadOnlyCollection<MembershipAdminDto>>> List(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!HasTenant(organizationId))
        {
            return Forbid();
        }

        return Ok(await listMembers.ExecuteAsync(organizationId, cancellationToken));
    }

    [Authorize(Policy = PermissionNames.MembershipsRead)]
    [HttpGet("invitations")]
    public async Task<ActionResult<IReadOnlyCollection<InvitationDto>>> ListInvitations(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!HasTenant(organizationId))
        {
            return Forbid();
        }

        return Ok(await invitations.ListAsync(organizationId, cancellationToken));
    }

    [Authorize(Policy = PermissionNames.MembershipsWrite)]
    [HttpPost("invitations")]
    public async Task<ActionResult<InvitationDto>> Invite(
        Guid organizationId,
        CreateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasTenant(organizationId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await inviteMember.ExecuteAsync(userId, organizationId, request, cancellationToken);
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        var status = result.Error.Code.Contains("conflict", StringComparison.Ordinal)
            ? StatusCodes.Status409Conflict
            : result.Error.Code.EndsWith("forbidden", StringComparison.Ordinal)
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize(Policy = PermissionNames.MembershipsWrite)]
    [HttpPatch("members/{membershipId:guid}")]
    public async Task<IActionResult> Update(
        Guid organizationId,
        Guid membershipId,
        UpdateMembershipRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasTenant(organizationId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await updateMembership.ExecuteAsync(
            userId,
            organizationId,
            membershipId,
            request,
            cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(
                statusCode: result.Error.Code.Contains("protected", StringComparison.Ordinal)
                    || result.Error.Code.Contains("forbidden", StringComparison.Ordinal)
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
    }

    private bool HasTenant(Guid organizationId) =>
        User.TryGetTenantId(out var tenantId) && tenantId == organizationId;
}
