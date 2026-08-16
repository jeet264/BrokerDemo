using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Insurers;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Insurer panel: the current brokerage's insurers plus read-only global (system) insurers.
/// </summary>
[ApiController]
[Authorize]
[Route("api/insurers")]
public sealed class InsurersController : ControllerBase
{
    private readonly IInsurerService _insurerService;

    public InsurersController(IInsurerService insurerService)
    {
        _insurerService = insurerService;
    }

    /// <summary>Lists insurers visible to this brokerage (org-owned plus global).</summary>
    /// <remarks>
    /// Auth: any signed-in role.
    /// Tenant scope: query filter allows OrganizationId == current org OR OrganizationId == null.
    /// Anonymous callers see nothing because CurrentOrganizationId is 0.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InsurerListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InsurerListDto>>>> List(
        [FromQuery] InsurerListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _insurerService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<InsurerListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Returns one insurer by public id if it is global or belongs to this org.</summary>
    /// <remarks>
    /// Auth: any signed-in role.
    /// Tenant scope: same filter as List. Other-tenant org insurers return 404.
    /// </remarks>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InsurerDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InsurerDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _insurerService.GetByPublicIdAsync(publicId, cancellationToken);
        return Ok(ApiResponse<InsurerDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Creates an insurer owned by the current brokerage (never a global insurer).</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: OrganizationId stamped from JWT. Name/code must not collide with this org or a global insurer.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InsurerDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InsurerDetailsDto>>> Create(
        [FromBody] CreateInsurerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _insurerService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<InsurerDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Updates a brokerage-owned insurer. System insurers cannot be changed.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: must be this org's row. Global insurers return 403 (existence is already visible on the panel).
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InsurerDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InsurerDetailsDto>>> Update(
        Guid publicId,
        [FromBody] UpdateInsurerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _insurerService.UpdateAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<InsurerDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Hard-deletes a brokerage-owned insurer when no policies reference it.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin only (AdminOnly).
    /// Tenant scope: same as Update. 409 if any policy (checked without tenant filter) still points at the insurer.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _insurerService.DeleteAsync(publicId, cancellationToken);
        return Ok(ApiResponse.Ok("Insurer deleted.", HttpContext.TraceIdentifier));
    }
}
