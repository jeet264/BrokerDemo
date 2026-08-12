namespace BrokerOS.Application.Auth;

public sealed class AuthResponseDto
{
    public required string AccessToken { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    public required CurrentUserDto User { get; init; }
}
