using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Insurers;
using BrokerOS.Application.Security;
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
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InsurerListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InsurerListDto>>>> List(
        [FromQuery] InsurerListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _insurerService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<InsurerListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

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
