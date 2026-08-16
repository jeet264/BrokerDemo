using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.MyDay;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Morning briefing: the next actions for the signed-in broker, not a stats dashboard.
/// Inline Call / Mark Done / Send Follow-up record activity without opening a detail screen.
/// </summary>
[ApiController]
[Authorize]
[Route("api/my-day")]
public sealed class MyDayController : ControllerBase
{
    private readonly IMyDayService _myDayService;

    public MyDayController(IMyDayService myDayService)
    {
        _myDayService = myDayService;
    }

    /// <summary>Returns overdue, due-today, and upcoming-urgent work for the signed-in user (capped lists).</summary>
    /// <remarks>
    /// Auth: any signed-in role.
    /// Tenant scope: EF query filters restrict to JWT OrganizationId. Employees additionally see only
    /// work assigned to them (AssignmentScope.ForCurrentUser). "Today" is the IST calendar date.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<MyDayDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MyDayDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _myDayService.GetAsync(cancellationToken);
        return Ok(ApiResponse<MyDayDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Marks a task complete, or clears a renewal's next follow-up (does not roll over the policy).</summary>
    /// <remarks>
    /// Auth: any signed-in role that can see the item (CanUpdateAssignedWork is all three roles).
    /// Tenant scope: assignment-filtered load; missing/other-book ids return 404.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Complete(
        [FromBody] MyDayActionRequest request,
        CancellationToken cancellationToken)
    {
        await _myDayService.CompleteAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("Marked done.", HttpContext.TraceIdentifier));
    }

    /// <summary>Logs a call on the timeline. Opens the phone on the client; this call records that it happened.</summary>
    /// <remarks>
    /// Auth: any signed-in role that can see the item.
    /// Tenant scope: same as GET. Also stamps LastFollowUpAtUtc on renewals.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanCreateActivities)]
    [HttpPost("call")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Call(
        [FromBody] MyDayActionRequest request,
        CancellationToken cancellationToken)
    {
        await _myDayService.LogCallAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("Call logged.", HttpContext.TraceIdentifier));
    }

    /// <summary>Logs a WhatsApp/follow-up and pushes the next chase out two IST days.</summary>
    /// <remarks>
    /// Auth: any signed-in role that can see the item.
    /// Tenant scope: same as GET. Does not mark the renewal Renewed.
    /// </remarks>
    [Authorize(Policy = AuthPolicies.CanCreateActivities)]
    [HttpPost("follow-up")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> FollowUp(
        [FromBody] MyDayActionRequest request,
        CancellationToken cancellationToken)
    {
        await _myDayService.SendFollowUpAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("Follow-up recorded.", HttpContext.TraceIdentifier));
    }
}
