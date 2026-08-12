using BrokerOS.Application.Auth;

namespace BrokerOS.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthResponseDto> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken);

    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken);
}
