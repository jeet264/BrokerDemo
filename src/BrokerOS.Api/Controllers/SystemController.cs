using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

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
