using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Insurers;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Insurers;

public sealed class InsurerService : IInsurerService
{
    private readonly BrokerOsDbContext _dbContext;

    public InsurerService(BrokerOsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InsurerListDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Insurers
            .AsNoTracking()
            .Where(insurer => insurer.IsActive)
            .OrderBy(insurer => insurer.Name)
            .Select(insurer => new InsurerListDto
            {
                PublicId = insurer.PublicId,
                Name = insurer.Name,
                Code = insurer.Code,
                IsActive = insurer.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
