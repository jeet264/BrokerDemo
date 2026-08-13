using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Dashboard;
using BrokerOS.Application.Renewals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IRenewalService _renewalService;

    public DashboardController(IDashboardService dashboardService, IRenewalService renewalService)
    {
        _dashboardService = dashboardService;
        _renewalService = renewalService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAsync(cancellationToken);
        return Ok(ApiResponse<DashboardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("renewals")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RenewalDashboardDto>>> Renewals(CancellationToken cancellationToken)
    {
        var result = await _renewalService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<RenewalDashboardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
