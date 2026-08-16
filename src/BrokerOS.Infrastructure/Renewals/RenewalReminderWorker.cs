using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Notifications;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrokerOS.Infrastructure.Renewals;

/// <summary>
/// Creates milestone tasks and outbound reminder drafts for open renewals.
/// Notifications go through <see cref="INotificationSender"/> so a live WhatsApp provider
/// can replace the simulated sender without changing this worker.
/// </summary>
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
                var result = await ProcessAsync(stoppingToken);
                if (result.TasksCreated > 0 || result.NotificationsCreated > 0)
                {
                    _logger.LogInformation(
                        "Renewal reminder worker created {TaskCount} task(s) and {NotificationCount} simulated notification(s).",
                        result.TasksCreated,
                        result.NotificationsCreated);
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

    private async Task<WorkerResult> ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.CurrentUserIdentifier = "system";

        var today = _clock.Today;
        var renewals = await dbContext.Renewals
            .IgnoreQueryFilters()
            .Include(renewal => renewal.Organization)
            .Include(renewal => renewal.AssignedUser)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Client)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Insurer)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.AssignedUser)
            .Where(renewal => OpenStatuses.Contains(renewal.Status))
            .ToListAsync(cancellationToken);

        if (renewals.Count == 0)
        {
            return WorkerResult.Empty;
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

        var existingTasks = existingMilestones
            .Select(item => (item.RenewalId!.Value, item.ReminderMilestoneDays!.Value))
            .ToHashSet();

        var existingNotifications = await dbContext.Notifications
            .IgnoreQueryFilters()
            .Where(notification =>
                renewalIds.Contains(notification.RenewalId)
                && notification.ReminderMilestoneDays != null)
            .Select(notification => new { notification.RenewalId, notification.ReminderMilestoneDays })
            .ToListAsync(cancellationToken);

        var existingNotes = existingNotifications
            .Select(item => (item.RenewalId, item.ReminderMilestoneDays!.Value))
            .ToHashSet();

        var tasksCreated = 0;
        var notificationsCreated = 0;
        foreach (var renewal in renewals)
        {
            var daysRemaining = RenewalCalendar.DaysRemaining(renewal.RenewalDate, today);
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

                if (MilestoneDeduper.TryRegister(existingTasks, renewal.Id, milestoneDays))
                {
                    dbContext.Tasks.Add(MilestoneTaskFactory.Create(renewal, milestoneDays, _clock.UtcNow));
                    tasksCreated++;
                }

                if (!existingNotes.Add((renewal.Id, milestoneDays)))
                {
                    continue;
                }

                var draft = SimulatedNotificationFactory.CreateForMilestone(renewal, milestoneDays);
                await sender.SendAsync(
                    new Notification
                    {
                        OrganizationId = renewal.OrganizationId,
                        RenewalId = renewal.Id,
                        ClientId = draft.ClientId,
                        RecipientType = draft.RecipientType,
                        Channel = draft.Channel,
                        Subject = draft.Subject,
                        Body = draft.Body,
                        ReminderMilestoneDays = milestoneDays
                    },
                    cancellationToken);
                notificationsCreated++;
            }
        }

        if (tasksCreated > 0 || notificationsCreated > 0 || dbContext.ChangeTracker.HasChanges())
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                _logger.LogInformation("Renewal reminder worker skipped duplicate milestone tasks or notifications.");
                return WorkerResult.Empty;
            }
        }

        return new WorkerResult(tasksCreated, notificationsCreated);
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sql && (sql.Number is 2601 or 2627);

    private sealed record WorkerResult(int TasksCreated, int NotificationsCreated)
    {
        public static WorkerResult Empty { get; } = new(0, 0);
    }
}
