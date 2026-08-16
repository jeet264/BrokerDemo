namespace BrokerOS.Application.Auth;

/// <summary>Login/register payload. AccessToken is a JWT; put it in Authorization: Bearer for later calls.</summary>
public sealed class AuthResponseDto
{
    public required string AccessToken { get; init; }

    /// <summary>UTC instant when the token stops being valid (not a business DateOnly).</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    public required CurrentUserDto User { get; init; }
}
