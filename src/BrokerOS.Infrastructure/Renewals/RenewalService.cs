using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Renewals;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Renewals;

public sealed class RenewalService : IRenewalService
{
    private static readonly RenewalStatus[] OpenStatuses =
    [
        RenewalStatus.Upcoming,
        RenewalStatus.InProgress,
        RenewalStatus.QuotationPending,
        RenewalStatus.ClientDecisionPending,
        RenewalStatus.Overdue
    ];

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public RenewalService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<RenewalListDto>> ListAsync(RenewalListQuery query, CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var renewals = AccessibleRenewals().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            renewals = renewals.Where(renewal =>
                renewal.Policy.PolicyNumber.Contains(term)
                || renewal.Policy.Client.CompanyName.Contains(term)
                || renewal.Policy.Insurer.Name.Contains(term));
        }

        if (query.Status.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.Status == query.Status.Value);
        }

        if (query.Stage.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.CurrentStage == query.Stage.Value);
        }

        if (query.Priority.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.Priority == query.Priority.Value);
        }

        if (query.AssignedUserPublicId.HasValue)
        {
            renewals = renewals.Where(renewal =>
                renewal.AssignedUser != null
                && renewal.AssignedUser.PublicId == query.AssignedUserPublicId.Value);
        }

        if (query.ClientPublicId.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.Policy.Client.PublicId == query.ClientPublicId.Value);
        }

        if (query.FromDate.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.RenewalDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            renewals = renewals.Where(renewal => renewal.RenewalDate <= query.ToDate.Value);
        }

        if (query.DueWithinDays.HasValue)
        {
            var until = today.AddDays(query.DueWithinDays.Value);
            renewals = renewals.Where(renewal =>
                renewal.RenewalDate >= today
                && renewal.RenewalDate <= until
                && OpenStatuses.Contains(renewal.Status));
        }

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        renewals = ApplySort(renewals, query.SortBy, descending);

        var totalCount = await renewals.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var entities = await renewals
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RenewalListDto>
        {
            Items = entities.Select(renewal => MapList(renewal, today)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RenewalDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: true);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<RenewalDetailsDto> UpdateStatusAsync(
        Guid publicId,
        UpdateRenewalStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Status == RenewalStatus.Renewed)
        {
            return await CompleteAsync(publicId, new CompleteRenewalRequest { Notes = request.Notes }, cancellationToken);
        }

        if (request.Status == RenewalStatus.Lost)
        {
            return await MarkLostAsync(publicId, new MarkRenewalLostRequest { Reason = request.Notes }, cancellationToken);
        }

        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(renewal);

        var previous = renewal.Status;
        renewal.Status = request.Status;
        AppendNotes(renewal, request.Notes);
        AddActivity(renewal, ActivityType.StatusChanged, $"Renewal status changed from {previous} to {request.Status}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<RenewalDetailsDto> UpdateStageAsync(
        Guid publicId,
        UpdateRenewalStageRequest request,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(renewal);

        var previous = renewal.CurrentStage;
        renewal.CurrentStage = request.Stage;
        if (renewal.Status == RenewalStatus.Upcoming && request.Stage != RenewalStage.NotStarted)
        {
            renewal.Status = RenewalStatus.InProgress;
        }

        AppendNotes(renewal, request.Notes);
        AddActivity(renewal, ActivityType.StatusChanged, $"Renewal stage changed from {previous} to {request.Stage}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<RenewalDetailsDto> CreateFollowUpAsync(
        Guid publicId,
        CreateFollowUpRequest request,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(renewal);

        var now = _clock.UtcNow;
        renewal.LastFollowUpAtUtc = now;
        if (request.NextFollowUpAtUtc.HasValue)
        {
            renewal.NextFollowUpAtUtc = DateTime.SpecifyKind(request.NextFollowUpAtUtc.Value, DateTimeKind.Utc);
        }

        AddActivity(renewal, request.ActivityType, request.Description.Trim());

        var shouldCreateTask = request.CreateTask || request.NextFollowUpAtUtc.HasValue;
        if (shouldCreateTask)
        {
            var title = string.IsNullOrWhiteSpace(request.TaskTitle)
                ? "Follow up on renewal"
                : request.TaskTitle.Trim();
            var due = request.TaskDueDateUtc
                ?? request.NextFollowUpAtUtc
                ?? now.AddDays(1);

            var task = new WorkTask
            {
                OrganizationId = renewal.OrganizationId,
                RenewalId = renewal.Id,
                ClientId = renewal.Policy.ClientId,
                PolicyId = renewal.PolicyId,
                AssignedUserId = renewal.AssignedUserId,
                Title = title,
                Description = request.Description.Trim(),
                DueDateUtc = DateTime.SpecifyKind(due, DateTimeKind.Utc),
                Priority = TaskPriority.Medium,
                Status = WorkTaskStatus.Pending
            };
            _dbContext.Tasks.Add(task);
            AddActivity(renewal, ActivityType.TaskCreated, $"Task created: {title}");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<RenewalDetailsDto> CompleteAsync(
        Guid publicId,
        CompleteRenewalRequest request,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(renewal);

        renewal.Status = RenewalStatus.Renewed;
        renewal.CurrentStage = RenewalStage.Completed;
        AppendNotes(renewal, request.Notes);
        AddActivity(renewal, ActivityType.StatusChanged, "Renewal marked as renewed.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<RenewalDetailsDto> MarkLostAsync(
        Guid publicId,
        MarkRenewalLostRequest request,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(renewal);

        renewal.Status = RenewalStatus.Lost;
        AppendNotes(renewal, request.Reason);
        AddActivity(
            renewal,
            ActivityType.StatusChanged,
            string.IsNullOrWhiteSpace(request.Reason)
                ? "Renewal marked as lost."
                : $"Renewal marked as lost: {request.Reason.Trim()}");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetails(renewal, _clock.Today);
    }

    public async Task<IReadOnlyList<RenewalActivityDto>> ListActivitiesAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: true);

        return await _dbContext.Activities
            .AsNoTracking()
            .Include(activity => activity.User)
            .Where(activity => activity.RenewalId == renewal.Id)
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .Select(activity => new RenewalActivityDto
            {
                PublicId = activity.PublicId,
                ActivityType = activity.ActivityType.ToString(),
                Description = activity.Description,
                CreatedAtUtc = activity.CreatedAtUtc,
                UserName = activity.User.FullName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RenewalTaskDto>> ListTasksAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(publicId, cancellationToken, asNoTracking: true);

        var tasks = await _dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.AssignedUser)
            .Where(task => task.RenewalId == renewal.Id)
            .OrderBy(task => task.DueDateUtc)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapTask).ToList();
    }

    public async Task<RenewalDashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var in7 = today.AddDays(7);
        var in30 = today.AddDays(30);
        var in60 = today.AddDays(60);
        var approaching = today.AddDays(RenewalMilestones.ApproachingDays);

        var renewals = AccessibleRenewals().AsNoTracking();

        var overdue = await renewals.CountAsync(
            renewal => renewal.RenewalDate < today && OpenStatuses.Contains(renewal.Status),
            cancellationToken);
        var dueToday = await renewals.CountAsync(
            renewal => renewal.RenewalDate == today && OpenStatuses.Contains(renewal.Status),
            cancellationToken);
        var dueWithin7 = await renewals.CountAsync(
            renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in7 && OpenStatuses.Contains(renewal.Status),
            cancellationToken);
        var dueWithin30 = await renewals.CountAsync(
            renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in30 && OpenStatuses.Contains(renewal.Status),
            cancellationToken);
        var dueWithin60 = await renewals.CountAsync(
            renewal => renewal.RenewalDate >= today && renewal.RenewalDate <= in60 && OpenStatuses.Contains(renewal.Status),
            cancellationToken);
        var renewed = await renewals.CountAsync(renewal => renewal.Status == RenewalStatus.Renewed, cancellationToken);
        var lost = await renewals.CountAsync(renewal => renewal.Status == RenewalStatus.Lost, cancellationToken);

        var premiumAtRisk = await renewals
            .Where(renewal =>
                OpenStatuses.Contains(renewal.Status)
                && renewal.RenewalDate <= approaching)
            .SumAsync(renewal => (decimal?)renewal.Policy.Premium, cancellationToken) ?? 0m;

        return new RenewalDashboardDto
        {
            Overdue = overdue,
            DueToday = dueToday,
            DueWithin7Days = dueWithin7,
            DueWithin30Days = dueWithin30,
            DueWithin60Days = dueWithin60,
            Renewed = renewed,
            Lost = lost,
            PremiumAtRisk = premiumAtRisk
        };
    }

    private IQueryable<Renewal> AccessibleRenewals()
    {
        return _dbContext.Renewals
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Client)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Insurer)
            .Include(renewal => renewal.AssignedUser)
            .ForCurrentUser(_currentUser);
    }

    private async Task<Renewal> GetAccessibleRenewalAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking)
    {
        var query = AccessibleRenewals().Where(renewal => renewal.PublicId == publicId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var renewal = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(renewal);
        return renewal!;
    }

    private static void EnsureOpen(Renewal renewal)
    {
        if (!RenewalFactory.IsOpen(renewal.Status))
        {
            throw new BusinessRuleException("This renewal is already closed.");
        }
    }

    private void AddActivity(Renewal renewal, ActivityType activityType, string description)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = renewal.OrganizationId,
            ClientId = renewal.Policy.ClientId,
            PolicyId = renewal.PolicyId,
            RenewalId = renewal.Id,
            UserId = _currentUser.UserId,
            ActivityType = activityType,
            Description = description
        });
    }

    private static void AppendNotes(Renewal renewal, string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return;
        }

        var incoming = notes.Trim();
        renewal.Notes = string.IsNullOrWhiteSpace(renewal.Notes)
            ? incoming
            : $"{renewal.Notes}{Environment.NewLine}{incoming}";
    }

    private static IQueryable<Renewal> ApplySort(IQueryable<Renewal> query, string? sortBy, bool descending)
    {
        return (sortBy?.Trim().ToLowerInvariant(), descending) switch
        {
            ("status", false) => query.OrderBy(renewal => renewal.Status),
            ("status", true) => query.OrderByDescending(renewal => renewal.Status),
            ("priority", false) => query.OrderBy(renewal => renewal.Priority),
            ("priority", true) => query.OrderByDescending(renewal => renewal.Priority),
            ("currentstage", false) => query.OrderBy(renewal => renewal.CurrentStage),
            ("currentstage", true) => query.OrderByDescending(renewal => renewal.CurrentStage),
            ("clientname", false) => query.OrderBy(renewal => renewal.Policy.Client.CompanyName),
            ("clientname", true) => query.OrderByDescending(renewal => renewal.Policy.Client.CompanyName),
            ("policynumber", false) => query.OrderBy(renewal => renewal.Policy.PolicyNumber),
            ("policynumber", true) => query.OrderByDescending(renewal => renewal.Policy.PolicyNumber),
            ("premium", false) => query.OrderBy(renewal => renewal.Policy.Premium),
            ("premium", true) => query.OrderByDescending(renewal => renewal.Policy.Premium),
            ("renewaldate", true) => query.OrderByDescending(renewal => renewal.RenewalDate),
            _ => query.OrderBy(renewal => renewal.RenewalDate)
        };
    }

    private static RenewalListDto MapList(Renewal renewal, DateOnly today) =>
        new()
        {
            PublicId = renewal.PublicId,
            PolicyPublicId = renewal.Policy.PublicId,
            PolicyNumber = renewal.Policy.PolicyNumber,
            PolicyType = renewal.Policy.PolicyType.ToString(),
            Premium = renewal.Policy.Premium,
            ExpiryDate = renewal.Policy.ExpiryDate,
            RenewalDate = renewal.RenewalDate,
            DaysRemaining = renewal.RenewalDate.DayNumber - today.DayNumber,
            Status = renewal.Status.ToString(),
            Priority = renewal.Priority.ToString(),
            CurrentStage = renewal.CurrentStage.ToString(),
            ClientName = renewal.Policy.Client.CompanyName,
            ClientPublicId = renewal.Policy.Client.PublicId,
            InsurerName = renewal.Policy.Insurer.Name,
            AssignedUserPublicId = renewal.AssignedUser?.PublicId,
            AssignedUserName = renewal.AssignedUser?.FullName,
            LastFollowUpAtUtc = renewal.LastFollowUpAtUtc,
            NextFollowUpAtUtc = renewal.NextFollowUpAtUtc
        };

    private static RenewalDetailsDto MapDetails(Renewal renewal, DateOnly today) =>
        new()
        {
            PublicId = renewal.PublicId,
            PolicyPublicId = renewal.Policy.PublicId,
            PolicyNumber = renewal.Policy.PolicyNumber,
            PolicyType = renewal.Policy.PolicyType.ToString(),
            PolicyStatus = renewal.Policy.Status.ToString(),
            Premium = renewal.Policy.Premium,
            SumInsured = renewal.Policy.SumInsured,
            StartDate = renewal.Policy.StartDate,
            ExpiryDate = renewal.Policy.ExpiryDate,
            RenewalDate = renewal.RenewalDate,
            DaysRemaining = renewal.RenewalDate.DayNumber - today.DayNumber,
            Status = renewal.Status.ToString(),
            Priority = renewal.Priority.ToString(),
            CurrentStage = renewal.CurrentStage.ToString(),
            ClientPublicId = renewal.Policy.Client.PublicId,
            ClientName = renewal.Policy.Client.CompanyName,
            InsurerPublicId = renewal.Policy.Insurer.PublicId,
            InsurerName = renewal.Policy.Insurer.Name,
            AssignedUserPublicId = renewal.AssignedUser?.PublicId,
            AssignedUserName = renewal.AssignedUser?.FullName,
            LastFollowUpAtUtc = renewal.LastFollowUpAtUtc,
            NextFollowUpAtUtc = renewal.NextFollowUpAtUtc,
            Notes = renewal.Notes,
            CreatedAtUtc = renewal.CreatedAtUtc,
            ModifiedAtUtc = renewal.ModifiedAtUtc,
            CreatedBy = renewal.CreatedBy,
            ModifiedBy = renewal.ModifiedBy
        };

    private static RenewalTaskDto MapTask(WorkTask task) =>
        new()
        {
            PublicId = task.PublicId,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            CompletedAtUtc = task.CompletedAtUtc,
            Priority = task.Priority.ToString(),
            Status = task.Status.ToString(),
            ReminderMilestoneDays = task.ReminderMilestoneDays,
            AssignedUserPublicId = task.AssignedUser?.PublicId,
            AssignedUserName = task.AssignedUser?.FullName,
            CreatedAtUtc = task.CreatedAtUtc,
            CreatedBy = task.CreatedBy
        };
}
