using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.QuickNotes;
using BrokerOS.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Fast desk capture: jot a note between calls without opening a full task form.
/// </summary>
/// <remarks>
/// Auth: any signed-in role (<c>CanCreateActivities</c>).
/// Tenant scope: OrganizationId from JWT. Linked client/renewal must be in-book (employees: assigned);
/// out of scope is 404, not 403. Unlinked notes are allowed.
/// Intentionally does not parse the text with AI/NLP — follow-up tasks are opt-in via
/// <c>createFollowUpTask</c>. That flag is the plug-in point for later intent detection
/// (alongside future AI document scanning).
/// </remarks>
[ApiController]
[Authorize(Policy = AuthPolicies.CanCreateActivities)]
[Route("api/quick-notes")]
public sealed class QuickNotesController : ControllerBase
{
    private readonly IQuickNoteService _quickNoteService;

    public QuickNotesController(IQuickNoteService quickNoteService)
    {
        _quickNoteService = quickNoteService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QuickNoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuickNoteDto>>> Create(
        [FromBody] CreateQuickNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _quickNoteService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<QuickNoteDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
