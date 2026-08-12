using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;
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
}
