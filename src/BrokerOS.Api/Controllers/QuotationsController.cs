using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Notifications;
using BrokerOS.Application.Quotations;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Manually logged insurer quotes on a renewal file. Not RFQ automation — the broker types in
/// what came back from a call or email, compares options, and shares a WhatsApp-style summary.
/// </summary>
/// <remarks>
/// Auth: list is any signed-in role; mutations use <c>CanUpdateAssignedWork</c>; share uses
/// <c>CanCreateActivities</c>. Tenant and assignment follow the parent renewal (404 if out of book).
/// Selecting a quotation enforces one chosen option per renewal and feeds Mark Renewed pre-fill.
/// Share goes through <c>INotificationSender</c> (simulated today).
/// </remarks>
[ApiController]
[Authorize]
[Route("api/quotations")]
public sealed class QuotationsController : ControllerBase
{
    private readonly IQuotationService _quotationService;

    public QuotationsController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> Update(
        Guid publicId,
        [FromBody] UpdateQuotationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _quotationService.UpdateAsync(publicId, request, cancellationToken);
        return Ok(ApiResponse<QuotationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Marks this quotation Selected and every other quotation on the same renewal Rejected.
    /// Only one chosen option is allowed per file.
    /// </summary>
    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpPut("{publicId:guid}/select")]
    [ProducesResponseType(typeof(ApiResponse<QuotationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuotationDto>>> Select(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _quotationService.SelectAsync(publicId, cancellationToken);
        return Ok(ApiResponse<QuotationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanUpdateAssignedWork)]
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid publicId, CancellationToken cancellationToken)
    {
        await _quotationService.DeleteAsync(publicId, cancellationToken);
        return Ok(ApiResponse.Ok("Quotation deleted.", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = AuthPolicies.CanCreateActivities)]
    [HttpPost("{publicId:guid}/share")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Share(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _quotationService.ShareAsync(publicId, cancellationToken);
        return Ok(ApiResponse<NotificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
