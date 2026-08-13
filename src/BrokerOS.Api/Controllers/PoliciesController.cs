using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/policies")]
public sealed class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PolicyListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PolicyListDto>>>> List(
        [FromQuery] PolicyListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _policyService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PolicyListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PolicyDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PolicyDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _policyService.GetByPublicIdAsync(publicId, cancellationToken);
        return Ok(ApiResponse<PolicyDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PolicyDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PolicyDetailsDto>>> Create(
        [FromBody] CreatePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _policyService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<PolicyDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PolicyDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PolicyDetailsDto>>> Update(
        Guid publicId,
        [FromBody] UpdatePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _policyService.UpdateAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<PolicyDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
