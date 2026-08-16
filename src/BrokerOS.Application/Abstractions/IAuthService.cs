using BrokerOS.Application.Auth;

namespace BrokerOS.Application.Abstractions;

/// <summary>Login, org registration, and current-user profile. Login/register run without tenant context.</summary>
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthResponseDto> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken);

    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken);
}
