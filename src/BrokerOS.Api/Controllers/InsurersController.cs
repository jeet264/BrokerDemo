using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Insurers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

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

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InsurerListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InsurerListDto>>>> List(CancellationToken cancellationToken)
    {
        var result = await _insurerService.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InsurerListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
