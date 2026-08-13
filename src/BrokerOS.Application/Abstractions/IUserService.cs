using BrokerOS.Application.Users;

namespace BrokerOS.Application.Abstractions;

public interface IUserService
{
    Task<IReadOnlyList<UserListDto>> ListAsync(CancellationToken cancellationToken);
}
