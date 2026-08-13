using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Dashboard;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private static readonly RenewalStatus[] OpenRenewalStatuses =
    [
        RenewalStatus.Upcoming,
        RenewalStatus.InProgress,
        RenewalStatus.QuotationPending,
        RenewalStatus.ClientDecisionPending,
        RenewalStatus.Overdue
    ];

    private static readonly WorkTaskStatus[] PendingTaskStatuses =
    [
        WorkTaskStatus.Pending,
        WorkTaskStatus.InProgress,
        WorkTaskStatus.Overdue
    ];

    private const int UpcomingLimit = 8;

    private const int TodaysTaskLimit = 8;

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public DashboardService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var startOfTodayUtc = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var startOfTomorrowUtc = startOfTodayUtc.AddDays(1);
        var in7 = today.AddDays(7);
        var in30 = today.AddDays(30);
        var in60 = today.AddDays(60);
        var approaching = today.AddDays(RenewalMilestones.ApproachingDays);

        var currentUserName = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == _currentUser.UserId)
            .Select(user => user.FullName)
            .SingleOrDefaultAsync(cancellationToken) ?? "there";

        var totalClients = await _dbContext.Clients
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .CountAsync(cancellationToken);

        var activePolicies = await _dbContext.Policies
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .CountAsync(policy => policy.Status == PolicyStatus.Active, cancellationToken);

        var pendingTasks = await _dbContext.Tasks
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .CountAsync(task => PendingTaskStatuses.Contains(task.Status), cancellationToken);

        var completedTasksToday = await _dbContext.Tasks
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .CountAsync(
                task => task.Status == WorkTaskStatus.Completed
                    && task.CompletedAtUtc != null
                    && task.CompletedAtUtc >= startOfTodayUtc
                    && task.CompletedAtUtc < startOfTomorrowUtc,
                cancellationToken);

        var openRenewals = await _dbContext.Renewals
            .AsNoTracking()
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Client)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Insurer)
            .Include(renewal => renewal.AssignedUser)
            .ForCurrentUser(_currentUser)
            .Where(renewal => OpenRenewalStatuses.Contains(renewal.Status))
            .ToListAsync(cancellationToken);

        var overdue = openRenewals.Count(renewal => renewal.RenewalDate < today);
        var dueToday = openRenewals.Count(renewal => renewal.RenewalDate == today);
        var dueWithin7 = openRenewals.Count(renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in7);
        var dueWithin30 = openRenewals.Count(renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in30);
        var dueWithin60 = openRenewals.Count(renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in60);
        var premiumAtRisk = openRenewals
            .Where(renewal => renewal.RenewalDate <= approaching)
            .Sum(renewal => renewal.Policy.Premium);
        var pendingFollowUps = openRenewals.Count(renewal => renewal.NextFollowUpAtUtc.HasValue);

        var upcoming = openRenewals
            .OrderBy(renewal => renewal.RenewalDate >= today)
            .ThenByDescending(renewal => renewal.Priority == RenewalPriority.Critical)
            .ThenByDescending(renewal => renewal.Priority)
            .ThenBy(renewal => renewal.Policy.ExpiryDate)
            .ThenBy(renewal => renewal.Policy.PolicyNumber)
            .Take(UpcomingLimit)
            .Select(renewal => MapUpcoming(renewal, today))
            .ToList();

        var todaysTasks = await _dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.Client)
            .Include(task => task.Policy)
            .Include(task => task.Renewal)
            .Include(task => task.AssignedUser)
            .ForCurrentUser(_currentUser)
            .Where(task =>
                PendingTaskStatuses.Contains(task.Status)
                && task.DueDateUtc < startOfTomorrowUtc)
            .ToListAsync(cancellationToken);

        var todaysTaskItems = todaysTasks
            .OrderBy(task => task.DueDateUtc >= startOfTodayUtc)
            .ThenByDescending(task => task.Priority == TaskPriority.Critical)
            .ThenByDescending(task => task.Priority)
            .ThenBy(task => task.DueDateUtc)
            .Take(TodaysTaskLimit)
            .Select(MapTask)
            .ToList();

        return new DashboardDto
        {
            CurrentUserName = currentUserName,
            TotalClients = totalClients,
            ActivePolicies = activePolicies,
            RenewalsOverdue = overdue,
            RenewalsDueToday = dueToday,
            RenewalsDueWithin7Days = dueWithin7,
            RenewalsDueWithin30Days = dueWithin30,
            RenewalsDueWithin60Days = dueWithin60,
            PremiumAtRisk = premiumAtRisk,
            PendingTasks = pendingTasks,
            CompletedTasksToday = completedTasksToday,
            PendingFollowUps = pendingFollowUps,
            UpcomingRenewals = upcoming,
            TodaysTasks = todaysTaskItems
        };
    }

    private static UpcomingRenewalDto MapUpcoming(Renewal renewal, DateOnly today)
    {
        var expiry = renewal.Policy.ExpiryDate;
        return new UpcomingRenewalDto
        {
            RenewalPublicId = renewal.PublicId,
            ClientName = renewal.Policy.Client.CompanyName,
            PolicyNumber = renewal.Policy.PolicyNumber,
            PolicyType = renewal.Policy.PolicyType.ToString(),
            InsurerName = renewal.Policy.Insurer.Name,
            Premium = renewal.Policy.Premium,
            ExpiryDate = expiry,
            DaysRemaining = expiry.DayNumber - today.DayNumber,
            Status = renewal.Status.ToString(),
            Priority = renewal.Priority.ToString(),
            AssignedUserName = renewal.AssignedUser?.FullName
        };
    }

    private static DashboardTaskDto MapTask(WorkTask task) =>
        new()
        {
            PublicId = task.PublicId,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            ClientName = task.Client?.CompanyName,
            PolicyNumber = task.Policy?.PolicyNumber,
            RenewalPublicId = task.Renewal?.PublicId,
            AssignedUserName = task.AssignedUser?.FullName
        };
}
