using BrokerOS.Application.Abstractions;
using BrokerOS.Application.MyDay;
using BrokerOS.Application.Security;
using BrokerOS.Application.Time;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.MyDay;

/// <summary>
/// Assembles the broker's morning list from open renewals and tasks in one briefing.
/// Two SQL queries (renewals + tasks) with projections — no N+1 per card.
/// </summary>
public sealed class MyDayService : IMyDayService
{
    private static readonly RenewalStatus[] ClosedRenewalStatuses =
    [
        RenewalStatus.Renewed,
        RenewalStatus.Lost,
        RenewalStatus.Cancelled
    ];

    private static readonly WorkTaskStatus[] ClosedTaskStatuses =
    [
        WorkTaskStatus.Completed,
        WorkTaskStatus.Cancelled
    ];

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public MyDayService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<MyDayDto> GetAsync(CancellationToken cancellationToken)
    {
        var utcNow = _clock.UtcNow;
        var today = IndiaBusinessCalendar.IstToday(utcNow);
        var upcomingEnd = today.AddDays(IndiaBusinessCalendar.UpcomingHorizonDays);

        var renewalRows = await _dbContext.Renewals
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .Where(renewal => !ClosedRenewalStatuses.Contains(renewal.Status))
            .Select(renewal => new RenewalQueryRow(
                renewal.Id,
                renewal.PublicId,
                renewal.RenewalDate,
                renewal.NextFollowUpAtUtc,
                renewal.Priority,
                renewal.CurrentStage,
                renewal.Policy.Premium,
                renewal.Policy.PolicyType,
                renewal.Policy.PublicId,
                renewal.Policy.PolicyNumber,
                renewal.Policy.Client.PublicId,
                renewal.Policy.Client.CompanyName,
                renewal.Policy.Client.Phone,
                renewal.Policy.ClientId,
                renewal.PolicyId,
                renewal.AssignedUserId))
            .ToListAsync(cancellationToken);

        var taskRows = await _dbContext.Tasks
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .Where(task => !ClosedTaskStatuses.Contains(task.Status))
            .Select(task => new TaskQueryRow(
                task.Id,
                task.PublicId,
                task.DueDateUtc,
                task.Priority,
                task.Title,
                task.Policy == null ? 0m : task.Policy.Premium,
                task.Policy == null ? null : task.Policy.PublicId,
                task.Policy == null ? null : task.Policy.PolicyNumber,
                task.Client != null ? task.Client.PublicId : task.Policy != null ? task.Policy.Client.PublicId : null,
                task.Client != null ? task.Client.CompanyName : task.Policy != null ? task.Policy.Client.CompanyName : null,
                task.Client != null ? task.Client.Phone : task.Policy != null ? task.Policy.Client.Phone : null,
                task.ClientId,
                task.PolicyId,
                task.RenewalId,
                task.AssignedUserId))
            .ToListAsync(cancellationToken);

        var classified = renewalRows
            .Select(row => ClassifyRenewal(ToSource(row), today, upcomingEnd))
            .Concat(taskRows.Select(row => ClassifyTask(ToSource(row), today, upcomingEnd)))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();

        var overdue = TakeBucket(classified, MyDayBucket.Overdue, CompareOverdue);
        var dueToday = TakeBucket(classified, MyDayBucket.DueToday, CompareDueToday);
        var upcoming = TakeBucket(classified, MyDayBucket.UpcomingUrgent, CompareUpcoming);

        return new MyDayDto
        {
            GeneratedAtUtc = utcNow,
            BusinessDate = today,
            OverdueItems = overdue.Items,
            OverdueTotalCount = overdue.Total,
            DueTodayItems = dueToday.Items,
            DueTodayTotalCount = dueToday.Total,
            UpcomingUrgentItems = upcoming.Items,
            UpcomingUrgentTotalCount = upcoming.Total
        };
    }

    public async Task CompleteAsync(MyDayActionRequest request, CancellationToken cancellationToken)
    {
        if (request.Kind == MyDayItemKind.Task)
        {
            var task = await LoadTaskAsync(request.PublicId, cancellationToken);
            if (task.Status is WorkTaskStatus.Completed or WorkTaskStatus.Cancelled)
            {
                return;
            }

            task.Status = WorkTaskStatus.Completed;
            task.CompletedAtUtc = _clock.UtcNow;
            AddActivity(
                ActivityType.TaskCompleted,
                $"Completed from My Day: {task.Title}",
                task.ClientId,
                task.PolicyId,
                task.RenewalId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        // Renewals are not "done" from My Day — that would skip rollover. Marking done here
        // only clears today's chase so the card leaves Due Today until the next follow-up is set.
        var renewal = await LoadRenewalAsync(request.PublicId, cancellationToken);
        renewal.LastFollowUpAtUtc = _clock.UtcNow;
        renewal.NextFollowUpAtUtc = null;
        AddActivity(
            ActivityType.StatusChanged,
            "Chase marked done from My Day (renewal still open).",
            renewal.Policy.ClientId,
            renewal.PolicyId,
            renewal.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogCallAsync(MyDayActionRequest request, CancellationToken cancellationToken)
    {
        var (clientId, policyId, renewalId, label) = await ResolveTargetAsync(request, cancellationToken);
        AddActivity(ActivityType.Call, $"Called from My Day: {label}", clientId, policyId, renewalId);

        if (request.Kind == MyDayItemKind.Renewal)
        {
            var renewal = await LoadRenewalAsync(request.PublicId, cancellationToken);
            renewal.LastFollowUpAtUtc = _clock.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SendFollowUpAsync(MyDayActionRequest request, CancellationToken cancellationToken)
    {
        var (clientId, policyId, renewalId, label) = await ResolveTargetAsync(request, cancellationToken);
        AddActivity(ActivityType.WhatsApp, $"Follow-up sent from My Day: {label}", clientId, policyId, renewalId);

        if (request.Kind == MyDayItemKind.Renewal)
        {
            var renewal = await LoadRenewalAsync(request.PublicId, cancellationToken);
            renewal.LastFollowUpAtUtc = _clock.UtcNow;
            renewal.NextFollowUpAtUtc = ToUtcFromIstDate(IndiaBusinessCalendar.IstToday(_clock.UtcNow).AddDays(2));
        }
        else
        {
            var task = await LoadTaskAsync(request.PublicId, cancellationToken);
            if (task.Status == WorkTaskStatus.Pending)
            {
                task.Status = WorkTaskStatus.InProgress;
            }

            // Same IST +2 day rule as renewals — do not add two UTC days (that can skip an IST calendar day).
            task.DueDateUtc = ToUtcFromIstDate(IndiaBusinessCalendar.IstToday(_clock.UtcNow).AddDays(2));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /*
     * Prioritization (v1 — likely to change).
     *
     * Bucket (an item is in exactly one):
     *   1. Overdue — IST due date before today (renewal date, missed follow-up, or task due).
     *   2. Due today — due date is today, including a 7-day escalation milestone that lands today.
     *   3. Upcoming urgent — due date or 7-day-before-expiry milestone in the next 3 IST days.
     * Closed renewals (Renewed/Lost/Cancelled) and closed tasks (Completed/Cancelled) are excluded.
     *
     * Sort inside a bucket:
     *   Overdue: most days overdue first, then higher priority enum, then higher premium (tie-break only).
     *   Due today: higher priority, then earlier due date, then higher premium.
     *   Upcoming: soonest due date, then higher priority, then higher premium.
     *
     * Premium is a TIE-BREAK today, not a weight. A small overdue SME should still outrank a large
     * policy that is not yet due. When we add "weight by premium", change CompareOverdue — do not
     * silently sort overdue by money first.
     *
     * Cap: 15 cards per bucket. Totals are the uncapped counts for "View all".
     */
    private static ClassifiedItem? ClassifyRenewal(SourceRow row, DateOnly today, DateOnly upcomingEnd)
    {
        var followUpOn = row.FollowUpUtc is DateTime followUp
            ? IndiaBusinessCalendar.ToIstDate(followUp)
            : (DateOnly?)null;
        var escalationOn = row.NaturalDueOn.AddDays(-IndiaBusinessCalendar.EscalationLeadDays);

        DateOnly dueOn;
        string reason;
        if (row.NaturalDueOn < today)
        {
            dueOn = row.NaturalDueOn;
            reason = "expired";
        }
        else if (followUpOn is DateOnly missedFollowUp && missedFollowUp < today)
        {
            dueOn = missedFollowUp;
            reason = "follow-up missed";
        }
        else if (row.NaturalDueOn == today)
        {
            dueOn = today;
            reason = "renews today";
        }
        else if (followUpOn == today)
        {
            dueOn = today;
            reason = "follow-up due today";
        }
        else if (escalationOn == today && row.NaturalDueOn > today)
        {
            dueOn = today;
            reason = "7-day escalation starts today";
        }
        else if (followUpOn is DateOnly upcomingFollowUp && upcomingFollowUp > today && upcomingFollowUp <= upcomingEnd)
        {
            dueOn = upcomingFollowUp;
            reason = "follow-up coming up";
        }
        else if (row.NaturalDueOn > today && row.NaturalDueOn <= upcomingEnd)
        {
            dueOn = row.NaturalDueOn;
            reason = "renews soon";
        }
        else if (escalationOn > today && escalationOn <= upcomingEnd && row.NaturalDueOn > today)
        {
            dueOn = escalationOn;
            reason = "7-day escalation approaching";
        }
        else
        {
            return null;
        }

        var bucket = dueOn < today
            ? MyDayBucket.Overdue
            : dueOn == today
                ? MyDayBucket.DueToday
                : MyDayBucket.UpcomingUrgent;

        var daysOverdue = bucket == MyDayBucket.Overdue ? today.DayNumber - dueOn.DayNumber : (int?)null;
        var line = FormatRenewalAction(row, reason, daysOverdue);

        return ToClassified(row, bucket, dueOn, daysOverdue, line, includeMarkDone: true);
    }

    private static ClassifiedItem? ClassifyTask(SourceRow row, DateOnly today, DateOnly upcomingEnd)
    {
        var dueOn = row.NaturalDueOn;
        if (dueOn > upcomingEnd)
        {
            return null;
        }

        var bucket = dueOn < today
            ? MyDayBucket.Overdue
            : dueOn == today
                ? MyDayBucket.DueToday
                : MyDayBucket.UpcomingUrgent;

        var daysOverdue = bucket == MyDayBucket.Overdue ? today.DayNumber - dueOn.DayNumber : (int?)null;
        var who = string.IsNullOrWhiteSpace(row.ClientName) ? "this client" : row.ClientName;
        var policyBit = string.IsNullOrWhiteSpace(row.PolicyNumber) ? string.Empty : $" ({row.PolicyNumber})";
        var when = bucket == MyDayBucket.Overdue
            ? $"overdue {daysOverdue} day{(daysOverdue == 1 ? string.Empty : "s")}"
            : bucket == MyDayBucket.DueToday
                ? "due today"
                : $"due {dueOn:dd MMM}";
        var line = $"{row.Label}: {who}{policyBit} — {when}";

        return ToClassified(row, bucket, dueOn, daysOverdue, line, includeMarkDone: true);
    }

    private static ClassifiedItem ToClassified(
        SourceRow row,
        MyDayBucket bucket,
        DateOnly dueOn,
        int? daysOverdue,
        string actionNeeded,
        bool includeMarkDone)
    {
        var actions = new List<MyDayAction> { MyDayAction.ViewDetails };
        if (!string.IsNullOrWhiteSpace(row.ClientPhone))
        {
            actions.Insert(0, MyDayAction.Call);
        }

        if (includeMarkDone)
        {
            actions.Add(MyDayAction.MarkDone);
        }

        actions.Add(MyDayAction.SendFollowUp);

        return new ClassifiedItem(
            new MyDayItemDto
            {
                Kind = row.Kind,
                PublicId = row.PublicId,
                ClientPublicId = row.ClientPublicId,
                ClientName = row.ClientName,
                ClientPhone = row.ClientPhone,
                PolicyPublicId = row.PolicyPublicId,
                PolicyNumber = row.PolicyNumber,
                ActionNeeded = actionNeeded,
                Bucket = bucket,
                DueOn = dueOn,
                DaysOverdue = daysOverdue,
                Priority = row.PriorityName,
                Stage = row.Kind == MyDayItemKind.Renewal ? row.Label : null,
                AvailableActions = actions
            },
            row.PriorityRank,
            row.Premium);
    }

    private static string FormatRenewalAction(SourceRow row, string reason, int? daysOverdue)
    {
        var who = string.IsNullOrWhiteSpace(row.ClientName) ? "this client" : row.ClientName;
        var cover = string.IsNullOrWhiteSpace(row.PolicyTypeName) ? "policy" : row.PolicyTypeName.ToLowerInvariant();
        var policyBit = string.IsNullOrWhiteSpace(row.PolicyNumber) ? string.Empty : $" {row.PolicyNumber}";
        if (daysOverdue is int days)
        {
            // Missed chase is overdue even when the cover date has not passed — do not say "expired".
            if (reason == "follow-up missed")
            {
                return $"Follow up with {who} on {cover}{policyBit} — chase missed {days} day{(days == 1 ? string.Empty : "s")} ago";
            }

            return $"Call {who} — {cover}{policyBit} expired {days} day{(days == 1 ? string.Empty : "s")} ago";
        }

        return reason switch
        {
            "renews today" => $"Call {who} — {cover}{policyBit} renews today",
            "follow-up due today" => $"Follow up with {who} on {cover}{policyBit} (due today)",
            "7-day escalation starts today" => $"Start 7-day escalation: {who} {cover}{policyBit}",
            "follow-up coming up" => $"Follow up with {who} on {cover}{policyBit}",
            "renews soon" => $"Prepare renewal for {who} — {cover}{policyBit}",
            "7-day escalation approaching" => $"7-day escalation approaching: {who} {cover}{policyBit}",
            _ => $"Chase {who} — {cover}{policyBit}"
        };
    }

    /*
     * Sort comparers — keep overdue-by-age first. Premium is the last key on purpose.
     */
    private static int CompareOverdue(ClassifiedItem left, ClassifiedItem right)
    {
        var byAge = (right.Dto.DaysOverdue ?? 0).CompareTo(left.Dto.DaysOverdue ?? 0);
        if (byAge != 0)
        {
            return byAge;
        }

        var byPriority = right.PriorityRank.CompareTo(left.PriorityRank);
        if (byPriority != 0)
        {
            return byPriority;
        }

        return right.Premium.CompareTo(left.Premium);
    }

    private static int CompareDueToday(ClassifiedItem left, ClassifiedItem right)
    {
        var byPriority = right.PriorityRank.CompareTo(left.PriorityRank);
        if (byPriority != 0)
        {
            return byPriority;
        }

        return right.Premium.CompareTo(left.Premium);
    }

    private static int CompareUpcoming(ClassifiedItem left, ClassifiedItem right)
    {
        var byDate = left.Dto.DueOn.CompareTo(right.Dto.DueOn);
        if (byDate != 0)
        {
            return byDate;
        }

        var byPriority = right.PriorityRank.CompareTo(left.PriorityRank);
        if (byPriority != 0)
        {
            return byPriority;
        }

        return right.Premium.CompareTo(left.Premium);
    }

    private static (IReadOnlyList<MyDayItemDto> Items, int Total) TakeBucket(
        IReadOnlyList<ClassifiedItem> all,
        MyDayBucket bucket,
        Comparison<ClassifiedItem> comparison)
    {
        var matched = all.Where(item => item.Dto.Bucket == bucket).ToList();
        matched.Sort(comparison);
        return (
            matched.Take(IndiaBusinessCalendar.MyDayListCap).Select(item => item.Dto).ToList(),
            matched.Count);
    }

    private async Task<WorkTask> LoadTaskAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == publicId, cancellationToken);
        AssignmentScope.EnsureFound(task);
        AssignmentScope.EnsureCanAccessAssigned(_currentUser, task!.AssignedUserId);
        return task;
    }

    private async Task<Renewal> LoadRenewalAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var renewal = await _dbContext.Renewals
            .Include(entity => entity.Policy)
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == publicId, cancellationToken);
        AssignmentScope.EnsureFound(renewal);
        AssignmentScope.EnsureCanAccessAssigned(_currentUser, renewal!.AssignedUserId);
        return renewal;
    }

    private async Task<(long? ClientId, long? PolicyId, long? RenewalId, string Label)> ResolveTargetAsync(
        MyDayActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Kind == MyDayItemKind.Task)
        {
            var task = await LoadTaskAsync(request.PublicId, cancellationToken);
            return (task.ClientId, task.PolicyId, task.RenewalId, task.Title);
        }

        var renewal = await LoadRenewalAsync(request.PublicId, cancellationToken);
        return (renewal.Policy.ClientId, renewal.PolicyId, renewal.Id, renewal.Policy.PolicyNumber);
    }

    private void AddActivity(
        ActivityType type,
        string description,
        long? clientId,
        long? policyId,
        long? renewalId)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientId = clientId,
            PolicyId = policyId,
            RenewalId = renewalId,
            UserId = _currentUser.UserId,
            ActivityType = type,
            Description = description
        });
    }

    private static SourceRow ToSource(RenewalQueryRow row) =>
        new(
            MyDayItemKind.Renewal,
            row.Id,
            row.PublicId,
            row.RenewalDate,
            row.NextFollowUpAtUtc,
            (int)row.Priority,
            row.Priority.ToString(),
            row.CurrentStage.ToString(),
            row.Premium,
            row.PolicyType.ToString(),
            row.PolicyPublicId,
            row.PolicyNumber,
            row.ClientPublicId,
            row.ClientName,
            row.ClientPhone,
            row.ClientId,
            row.PolicyId,
            row.Id,
            row.AssignedUserId);

    private static SourceRow ToSource(TaskQueryRow row) =>
        new(
            MyDayItemKind.Task,
            row.Id,
            row.PublicId,
            IndiaBusinessCalendar.ToIstDate(row.DueDateUtc),
            row.DueDateUtc,
            (int)row.Priority,
            row.Priority.ToString(),
            row.Title,
            row.Premium,
            null,
            row.PolicyPublicId,
            row.PolicyNumber,
            row.ClientPublicId,
            row.ClientName,
            row.ClientPhone,
            row.ClientId,
            row.PolicyId,
            row.RenewalId,
            row.AssignedUserId);

    /// <summary>Stores an IST calendar date as UTC noon-IST so the follow-up still falls on that IST day.</summary>
    private static DateTime ToUtcFromIstDate(DateOnly istDate)
    {
        var unspecifiedNoon = DateTime.SpecifyKind(istDate.ToDateTime(new TimeOnly(12, 0)), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedNoon, IndiaBusinessCalendar.TimeZone);
    }

    private sealed record RenewalQueryRow(
        long Id,
        Guid PublicId,
        DateOnly RenewalDate,
        DateTime? NextFollowUpAtUtc,
        RenewalPriority Priority,
        RenewalStage CurrentStage,
        decimal Premium,
        PolicyType PolicyType,
        Guid PolicyPublicId,
        string PolicyNumber,
        Guid ClientPublicId,
        string ClientName,
        string ClientPhone,
        long ClientId,
        long PolicyId,
        long? AssignedUserId);

    private sealed record TaskQueryRow(
        long Id,
        Guid PublicId,
        DateTime DueDateUtc,
        TaskPriority Priority,
        string Title,
        decimal Premium,
        Guid? PolicyPublicId,
        string? PolicyNumber,
        Guid? ClientPublicId,
        string? ClientName,
        string? ClientPhone,
        long? ClientId,
        long? PolicyId,
        long? RenewalId,
        long? AssignedUserId);

    private sealed record SourceRow(
        MyDayItemKind Kind,
        long Id,
        Guid PublicId,
        DateOnly NaturalDueOn,
        DateTime? FollowUpUtc,
        int PriorityRank,
        string PriorityName,
        string Label,
        decimal Premium,
        string? PolicyTypeName,
        Guid? PolicyPublicId,
        string? PolicyNumber,
        Guid? ClientPublicId,
        string? ClientName,
        string? ClientPhone,
        long? ClientId,
        long? PolicyId,
        long? RenewalId,
        long? AssignedUserId);

    private sealed record ClassifiedItem(MyDayItemDto Dto, int PriorityRank, decimal Premium);
}
