using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Auth;
using BrokerOS.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Sign-in, first-organization registration, and the current-user profile.
/// Login and register are anonymous; tenant context is empty until a JWT is issued.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticates a user and returns a JWT plus profile.</summary>
    /// <remarks>
    /// Auth: anonymous.
    /// Tenant scope: not applied yet. AuthService uses IgnoreQueryFilters to look up email globally,
    /// then the issued token's OrganizationId becomes the tenant for all later requests.
    /// Failed login always returns the same 401 message (no account-enumeration).
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Creates a new brokerage and its first BrokerAdmin, then signs that admin in.</summary>
    /// <remarks>
    /// Auth: anonymous.
    /// Tenant scope: creates a new Organization; email and organization code must be globally unique.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register-organization")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterOrganization(
        [FromBody] RegisterOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterOrganizationAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Returns the signed-in user and their brokerage (from the JWT).</summary>
    /// <remarks>
    /// Auth: any valid Bearer token.
    /// Tenant scope: looks up UserId from the token under the current org query filter.
    /// Deactivated users or orgs return 401 so the client drops the token.
    /// </remarks>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> Me(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}
