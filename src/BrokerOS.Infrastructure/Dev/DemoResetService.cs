using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Dev;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using BrokerOS.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrokerOS.Infrastructure.Dev;

public sealed class DemoResetService : IDemoResetService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly DevelopmentDataSeeder _seeder;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DemoResetService> _logger;
    private readonly bool _enableDemoReset;
    private readonly bool _seedDemoDataOnStartup;

    public DemoResetService(
        BrokerOsDbContext dbContext,
        DevelopmentDataSeeder seeder,
        IHostEnvironment environment,
        IConfiguration configuration,
        ILogger<DemoResetService> logger)
    {
        _dbContext = dbContext;
        _seeder = seeder;
        _environment = environment;
        _logger = logger;
        _enableDemoReset = configuration.GetValue("BrokerOS:EnableDemoReset", false);
        _seedDemoDataOnStartup = configuration.GetValue("BrokerOS:SeedDemoDataOnStartup", false);
    }

    public bool IsEnabled =>
        _enableDemoReset && (_environment.IsDevelopment() || _seedDemoDataOnStartup);

    public async Task<DemoResetSummaryDto> ResetAsync(CancellationToken cancellationToken)
    {
        EnsureEnabled();

        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.Code == DevelopmentDataSeeder.DemoOrganizationCode, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (organization is not null)
            {
                await WipeOrganizationDataAsync(organization.Id, cancellationToken);
            }

            _dbContext.ChangeTracker.Clear();
            await _seeder.SeedAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation("Development demo data was reset for {Code}.", DevelopmentDataSeeder.DemoOrganizationCode);
        return await SummarizeAsync(cancellationToken);
    }

    private void EnsureEnabled()
    {
        if (!IsEnabled)
        {
            throw new NotFoundException("The requested resource was not found.");
        }
    }

    private async Task WipeOrganizationDataAsync(long organizationId, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Activities.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Tasks.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Quotations.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Renewals.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Policies.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(policy => policy.NextPolicyId, (long?)null)
                    .SetProperty(policy => policy.PreviousPolicyId, (long?)null),
                cancellationToken);

        await _dbContext.Policies.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Contacts.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Clients.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Insurers.IgnoreQueryFilters()
            .Where(entity => entity.OrganizationId == organizationId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<DemoResetSummaryDto> SummarizeAsync(CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.Code == DevelopmentDataSeeder.DemoOrganizationCode, cancellationToken);

        var organizationId = organization.Id;

        return new DemoResetSummaryDto
        {
            OrganizationName = organization.Name,
            OrganizationCode = organization.Code,
            Clients = await _dbContext.Clients.IgnoreQueryFilters()
                .CountAsync(client => client.OrganizationId == organizationId && !client.IsDeleted, cancellationToken),
            Policies = await _dbContext.Policies.IgnoreQueryFilters()
                .CountAsync(policy => policy.OrganizationId == organizationId && !policy.IsDeleted, cancellationToken),
            Renewals = await _dbContext.Renewals.IgnoreQueryFilters()
                .CountAsync(renewal => renewal.OrganizationId == organizationId, cancellationToken),
            Users = await _dbContext.Users.IgnoreQueryFilters()
                .CountAsync(user => user.OrganizationId == organizationId && !user.IsDeleted, cancellationToken),
            Insurers = await _dbContext.Insurers.IgnoreQueryFilters()
                .CountAsync(
                    insurer => insurer.OrganizationId == organizationId || insurer.OrganizationId == null,
                    cancellationToken),
            Tasks = await _dbContext.Tasks.IgnoreQueryFilters()
                .CountAsync(task => task.OrganizationId == organizationId && !task.IsDeleted, cancellationToken)
        };
    }
}
