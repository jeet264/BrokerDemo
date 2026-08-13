using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
