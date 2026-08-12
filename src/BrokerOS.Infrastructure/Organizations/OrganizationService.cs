using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Organizations;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Organizations;

public sealed class OrganizationService : IOrganizationService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public OrganizationService(BrokerOsDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<OrganizationDetailsDto> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("The requested resource was not found.");
        }

        return Map(organization);
    }

    public async Task<OrganizationDetailsDto> UpdateCurrentAsync(
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.BrokerAdmin)
        {
            throw new ForbiddenException("Only a broker admin can update organization settings.");
        }

        var organization = await _dbContext.Organizations
            .SingleOrDefaultAsync(entity => entity.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("The requested resource was not found.");
        }

        organization.Name = request.Name.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    private static OrganizationDetailsDto Map(Domain.Entities.Organization organization) =>
        new()
        {
            PublicId = organization.PublicId,
            Name = organization.Name,
            Code = organization.Code,
            IsActive = organization.IsActive
        };
}
