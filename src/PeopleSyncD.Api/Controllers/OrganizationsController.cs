using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Application.DTOs;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Application.Organizations;

namespace PeopleSyncD.Api.Controllers;

/// <summary>
/// Organization lifecycle endpoints.
/// </summary>
[ApiController]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(
    IOrganizationRepository repository,
    CreateOrganizationService createService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrganizationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganizationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization = await repository.GetByIdAsync(id, cancellationToken);
        return organization is null
            ? NotFound()
            : Ok(new OrganizationDto(
                organization.Id,
                organization.Name,
                organization.Slug,
                organization.CreatedAt));
    }

    [HttpPost]
    [ProducesResponseType<OrganizationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrganizationDto>> Create(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createService.ExecuteAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        var status = result.Error.Code == "organization.slug_conflict"
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }
}
