using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClientListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ClientListDto>>>> List(
        [FromQuery] ClientListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ClientListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClientDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClientDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.GetByPublicIdAsync(publicId, cancellationToken);
        return Ok(ApiResponse<ClientDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClientDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClientDetailsDto>>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<ClientDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClientDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClientDetailsDto>>> Update(
        Guid publicId,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.UpdateAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<ClientDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _clientService.DeleteAsync(publicId, cancellationToken);
        return Ok(ApiResponse.Ok("Client deleted.", HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/policies")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ClientPolicyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientPolicyDto>>>> ListPolicies(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.ListPoliciesAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClientPolicyDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/renewals")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ClientRenewalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientRenewalDto>>>> ListRenewals(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.ListRenewalsAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClientRenewalDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/activities")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ClientActivityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientActivityDto>>>> ListActivities(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.ListActivitiesAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClientActivityDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
