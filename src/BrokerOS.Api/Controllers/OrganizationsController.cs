using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Organizations;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Current-brokerage profile. There is no org-id in the route — the tenant is always the JWT OrganizationId.
/// </summary>
[ApiController]
[Authorize]
[Route("api/organizations")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>Returns the signed-in user's brokerage.</summary>
    /// <remarks>
    /// Auth: any signed-in role.
    /// Tenant scope: Organization query filter is Id == JWT OrganizationId, so this cannot return another tenant.
    /// </remarks>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrganizationDetailsDto>>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<OrganizationDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Updates the display name of the signed-in user's brokerage.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin (CanManageOrganization). The service repeats the role check.
    /// Tenant scope: updates only JWT OrganizationId. Organization code is not changeable here (unique registration key).
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanManageOrganization)]
    [HttpPut("current")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrganizationDetailsDto>>> UpdateCurrent(
        [FromBody] UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _organizationService.UpdateCurrentAsync(request, cancellationToken);
        return Ok(ApiResponse<OrganizationDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
