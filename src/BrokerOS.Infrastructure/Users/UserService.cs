using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Users;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Users;

public sealed class UserService : IUserService
{
    private readonly BrokerOsDbContext _dbContext;

    public UserService(BrokerOsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserListDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .Select(user => new UserListDto
            {
                PublicId = user.PublicId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
