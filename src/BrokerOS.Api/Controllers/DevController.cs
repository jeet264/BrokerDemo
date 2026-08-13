using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Dev;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/dev")]
public sealed class DevController : ControllerBase
{
    private readonly IDemoResetService _demoResetService;

    public DevController(IDemoResetService demoResetService)
    {
        _demoResetService = demoResetService;
    }

    [HttpPost("reset-demo-data")]
    [ProducesResponseType(typeof(ApiResponse<DemoResetSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DemoResetSummaryDto>>> ResetDemoData(
        CancellationToken cancellationToken)
    {
        var result = await _demoResetService.ResetAsync(cancellationToken);
        var message =
            $"Reloaded sample data: {result.Clients} clients, {result.Policies} policies, {result.Renewals} renewals.";
        return Ok(ApiResponse<DemoResetSummaryDto>.Ok(result, message, HttpContext.TraceIdentifier));
    }
}
