using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.DTOs;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Permissions;

namespace PeopleSyncD.Api.Controllers;

/// <summary>
/// Tenant-scoped organization endpoints.
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(IOrganizationRepository repository) : ControllerBase
{
    [Authorize(Policy = PermissionNames.OrganizationsRead)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrganizationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId) || tenantId != id)
        {
            return Forbid();
        }

        var organization = await repository.GetByIdAsync(id, cancellationToken);
        return organization is null
            ? NotFound()
            : Ok(new OrganizationDto(
                organization.Id,
                organization.Name,
                organization.Slug,
                organization.CreatedAt));
    }
}
