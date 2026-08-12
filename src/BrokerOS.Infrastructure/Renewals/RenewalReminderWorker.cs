using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrokerOS.Infrastructure.Renewals;

public sealed class RenewalReminderWorker : BackgroundService
{
    private static readonly RenewalStatus[] OpenStatuses =
    [
        RenewalStatus.Upcoming,
        RenewalStatus.InProgress,
        RenewalStatus.QuotationPending,
        RenewalStatus.ClientDecisionPending,
        RenewalStatus.Overdue
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IClock _clock;
    private readonly ILogger<RenewalReminderWorker> _logger;
    private readonly RenewalWorkerOptions _options;

    public RenewalReminderWorker(
        IServiceScopeFactory scopeFactory,
        IClock clock,
        IOptions<RenewalWorkerOptions> options,
        ILogger<RenewalReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _clock = clock;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Renewal reminder worker is disabled.");
            return;
        }

        var startupDelay = TimeSpan.FromSeconds(Math.Max(0, _options.StartupDelaySeconds));
        if (startupDelay > TimeSpan.Zero)
        {
            await Task.Delay(startupDelay, stoppingToken);
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var created = await ProcessAsync(stoppingToken);
                if (created > 0)
                {
                    _logger.LogInformation("Renewal reminder worker created {Count} task(s).", created);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Renewal reminder worker failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task<int> ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.CurrentUserIdentifier = "system";

        var today = _clock.Today;
        var renewals = await dbContext.Renewals
            .IgnoreQueryFilters()
            .Include(renewal => renewal.Policy)
            .Where(renewal => OpenStatuses.Contains(renewal.Status))
            .ToListAsync(cancellationToken);

        if (renewals.Count == 0)
        {
            return 0;
        }

        var renewalIds = renewals.Select(renewal => renewal.Id).ToList();
        var existingMilestones = await dbContext.Tasks
            .IgnoreQueryFilters()
            .Where(task =>
                task.RenewalId != null
                && renewalIds.Contains(task.RenewalId.Value)
                && task.ReminderMilestoneDays != null
                && !task.IsDeleted)
            .Select(task => new { task.RenewalId, task.ReminderMilestoneDays })
            .ToListAsync(cancellationToken);

        var existing = existingMilestones
            .Select(item => (item.RenewalId!.Value, item.ReminderMilestoneDays!.Value))
            .ToHashSet();

        var created = 0;
        foreach (var renewal in renewals)
        {
            var daysRemaining = renewal.RenewalDate.DayNumber - today.DayNumber;
            if (daysRemaining < 0 && renewal.Status == RenewalStatus.Upcoming)
            {
                renewal.Status = RenewalStatus.Overdue;
            }

            renewal.Priority = RenewalMilestones.RenewalPriorityFor(daysRemaining);

            foreach (var milestoneDays in RenewalMilestones.Days)
            {
                if (daysRemaining > milestoneDays)
                {
                    continue;
                }

                if (!existing.Add((renewal.Id, milestoneDays)))
                {
                    continue;
                }

                var dueDate = renewal.RenewalDate.AddDays(-milestoneDays).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                if (dueDate < _clock.UtcNow)
                {
                    dueDate = _clock.UtcNow;
                }

                dbContext.Tasks.Add(new WorkTask
                {
                    OrganizationId = renewal.OrganizationId,
                    RenewalId = renewal.Id,
                    ClientId = renewal.Policy.ClientId,
                    PolicyId = renewal.PolicyId,
                    AssignedUserId = renewal.AssignedUserId ?? renewal.Policy.AssignedUserId,
                    Title = RenewalMilestones.TaskTitle(milestoneDays),
                    Description = $"Policy {renewal.Policy.PolicyNumber} renews on {renewal.RenewalDate:yyyy-MM-dd}.",
                    DueDateUtc = dueDate,
                    Priority = RenewalMilestones.TaskPriorityFor(milestoneDays),
                    Status = WorkTaskStatus.Pending,
                    ReminderMilestoneDays = milestoneDays,
                    CreatedBy = "system"
                });
                created++;
            }
        }

        if (created > 0 || dbContext.ChangeTracker.HasChanges())
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                _logger.LogInformation("Renewal reminder worker skipped duplicate milestone tasks.");
                return 0;
            }
        }

        return created;
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sql && (sql.Number is 2601 or 2627);
}
