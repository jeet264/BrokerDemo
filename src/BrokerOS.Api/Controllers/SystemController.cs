using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Anonymous health/status for the dashboard shell. Does not read tenant data.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly IClock _clock;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SystemController(IClock clock, IHostEnvironment environment, IConfiguration configuration)
    {
        _clock = clock;
        _environment = environment;
        _configuration = configuration;
    }

    /// <summary>Returns product metadata, UTC now, and whether a SQL connection string is configured.</summary>
    /// <remarks>
    /// Auth: anonymous (also used by the unauthenticated dashboard).
    /// Tenant scope: none. DatabaseConfigured only means a connection string exists, not that SQL is reachable.
    /// UtcNow is UTC; the UI converts to IST for display.
    /// </remarks>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<SystemStatusDto>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<SystemStatusDto>> GetStatus()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        var status = new SystemStatusDto
        {
            ProductName = _configuration["BrokerOS:ProductName"] ?? "BrokerOS",
            Tagline = _configuration["BrokerOS:Tagline"] ?? "Insurance Broker Operations & Renewal Automation Platform",
            Environment = _environment.EnvironmentName,
            ApiVersion = "0.1.0",
            UtcNow = _clock.UtcNow,
            DatabaseConfigured = !string.IsNullOrWhiteSpace(connectionString)
        };

        return Ok(ApiResponse<SystemStatusDto>.Ok(status, traceId: HttpContext.TraceIdentifier));
    }
}
