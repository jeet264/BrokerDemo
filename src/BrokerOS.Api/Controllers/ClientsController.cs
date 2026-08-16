using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// HTTP surface for the client book. Tenant isolation is the JWT OrganizationId via query filters;
/// employees additionally see only assigned clients.
/// </summary>
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

    /// <summary>Lists clients in the current brokerage, with search, filters, and paging.</summary>
    /// <remarks>
    /// Auth: any signed-in role (BrokerAdmin, BrokerManager, BrokerEmployee).
    /// Tenant scope: EF query filters restrict rows to JWT OrganizationId.
    /// Employees see only clients assigned to them (AssignmentScope.ForCurrentUser).
    /// Cross-tenant or out-of-scope ids return 404, not 403.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClientListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ClientListDto>>>> List(
        [FromQuery] ClientListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _clientService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ClientListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Returns one client by public id.</summary>
    /// <remarks>
    /// Auth: any signed-in role.
    /// Tenant scope: JWT OrganizationId plus assignment scope. Unknown/other-tenant/unassigned → 404.
    /// </remarks>
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

    /// <summary>Creates a client in the signed-in user's brokerage.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: OrganizationId is taken from the JWT in ClientService, never from the body.
    /// ClientCode must be unique among non-deleted clients in this org.
    /// </remarks>
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

    /// <summary>Updates a client in the current brokerage.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: row must belong to JWT OrganizationId and be visible under assignment rules → otherwise 404.
    /// </remarks>
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

    /// <summary>Soft-deletes a client (IsDeleted). Historical policies remain in the database.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: same as GET — missing or other-tenant ids return 404.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanManageOperations)]
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _clientService.DeleteAsync(publicId, cancellationToken);
        return Ok(ApiResponse.Ok("Client deleted.", HttpContext.TraceIdentifier));
    }

    /// <summary>Lists policy terms for a client (including historical terms once rollover exists).</summary>
    /// <remarks>
    /// Auth: any signed-in role that can GET the client.
    /// Tenant scope: client must be accessible first; policies are then filtered by that client's internal Id (already org-scoped).
    /// StartDate/ExpiryDate are DateOnly (yyyy-MM-dd on the wire).
    /// </remarks>
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

    /// <summary>Lists renewal workflow records for policies belonging to this client.</summary>
    /// <remarks>
    /// Auth: any signed-in role that can GET the client.
    /// Tenant scope: same as ListPolicies — inaccessible client → 404.
    /// </remarks>
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

    /// <summary>Lists the activity timeline for a client (newest first).</summary>
    /// <remarks>
    /// Auth: any signed-in role that can GET the client.
    /// Tenant scope: same as GET client. CreatedAtUtc is UTC; display in IST in the UI.
    /// </remarks>
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
