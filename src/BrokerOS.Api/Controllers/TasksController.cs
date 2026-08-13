using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using BrokerOS.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TaskListDto>>>> List(
        [FromQuery] TaskListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TaskListDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.GetByPublicIdAsync(publicId, cancellationToken);
        return Ok(ApiResponse<TaskDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDetailsDto>>> Update(
        Guid publicId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDetailsDto>>> Complete(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.CompleteAsync(publicId, cancellationToken);
        return Ok(ApiResponse<TaskDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/reassign")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDetailsDto>>> Reassign(
        Guid publicId,
        [FromBody] ReassignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.ReassignAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<TaskDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<TaskDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDetailsDto>>> Cancel(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.CancelAsync(publicId, cancellationToken);
        return Ok(ApiResponse<TaskDetailsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
