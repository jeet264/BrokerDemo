using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Notifications;
using BrokerOS.Application.Renewals;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/renewals")]
public sealed class RenewalsController : ControllerBase
{
    private readonly IRenewalService _renewalService;
    private readonly INotificationService _notificationService;

    public RenewalsController(IRenewalService renewalService, INotificationService notificationService)
    {
        _renewalService = renewalService;
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RenewalListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RenewalListDto>>>> List(
        [FromQuery] RenewalListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<RenewalListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.GetByPublicIdAsync(publicId, cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> UpdateStatus(
        Guid publicId,
        [FromBody] UpdateRenewalStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.UpdateStatusAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/stage")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> UpdateStage(
        Guid publicId,
        [FromBody] UpdateRenewalStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.UpdateStageAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanCreateActivities)]
    [HttpPost("{publicId:guid}/follow-up")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> FollowUp(
        Guid publicId,
        [FromBody] CreateFollowUpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.CreateFollowUpAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanCreateActivities)]
    [HttpPost("{publicId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> CreateTask(
        Guid publicId,
        [FromBody] CreateRenewalTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.CreateTaskAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPost("{publicId:guid}/complete")]
    [HttpPut("{publicId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> Complete(
        Guid publicId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CompleteRenewalRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.CompleteAsync(
            publicId,
            request ?? new CompleteRenewalRequest(),
            cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPost("{publicId:guid}/lost")]
    [HttpPut("{publicId:guid}/lost")]
    [ProducesResponseType(typeof(ApiResponse<RenewalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RenewalDetailsDto>>> MarkLost(
        Guid publicId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] MarkRenewalLostRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.MarkLostAsync(
            publicId,
            request ?? new MarkRenewalLostRequest(),
            cancellationToken);
        return Ok(ApiResponse<RenewalDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/activities")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RenewalActivityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RenewalActivityDto>>>> ListActivities(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.ListActivitiesAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RenewalActivityDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RenewalTaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RenewalTaskDto>>>> ListTasks(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _renewalService.ListTasksAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RenewalTaskDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}/notifications")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> ListNotifications(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.ListForRenewalAsync(publicId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
