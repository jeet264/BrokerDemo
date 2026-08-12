using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Organizations;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

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

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrganizationDetailsDto>>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<OrganizationDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

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
