using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// One search box for the desk: client name/phone and policy number/vehicle number.
/// </summary>
/// <remarks>
/// Auth: any signed-in role.
/// Tenant scope: EF query filters + assignment (employees only see their book).
/// Contains/LIKE today — swap the implementation inside <c>SearchService.SearchAsync</c> for
/// full-text or fuzzy match later; this endpoint stays the same.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SearchResultsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SearchResultsDto>>> Search(
        [FromQuery] SearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAsync(query.Q, cancellationToken);
        return Ok(ApiResponse<SearchResultsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
