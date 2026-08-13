using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using BrokerOS.Application.Tasks;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.WorkTasks;

public sealed class TaskService : ITaskService
{
    private static readonly WorkTaskStatus[] OpenStatuses =
    [
        WorkTaskStatus.Pending,
        WorkTaskStatus.InProgress,
        WorkTaskStatus.Overdue
    ];

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public TaskService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<TaskListDto>> ListAsync(TaskListQuery query, CancellationToken cancellationToken)
    {
        var utcNow = _clock.UtcNow;
        var tasks = AccessibleTasks().AsNoTracking();
        tasks = ApplyView(tasks, query.View, utcNow);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            tasks = tasks.Where(task =>
                task.Title.Contains(term)
                || (task.Description != null && task.Description.Contains(term))
                || (task.Client != null && task.Client.CompanyName.Contains(term))
                || (task.Policy != null && task.Policy.PolicyNumber.Contains(term)));
        }

        if (query.Status.HasValue)
        {
            tasks = tasks.Where(task => task.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            tasks = tasks.Where(task => task.Priority == query.Priority.Value);
        }

        if (query.AssignedUserPublicId.HasValue)
        {
            tasks = tasks.Where(task =>
                task.AssignedUser != null
                && task.AssignedUser.PublicId == query.AssignedUserPublicId.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromUtc = query.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            tasks = tasks.Where(task => task.DueDateUtc >= fromUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toUtc = query.ToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            tasks = tasks.Where(task => task.DueDateUtc < toUtc);
        }

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        tasks = ApplySort(tasks, query.SortBy, descending);

        var totalCount = await tasks.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var entities = await tasks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskListDto>
        {
            Items = entities.Select(task => MapList(task, utcNow)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var task = await GetAccessibleTaskAsync(publicId, cancellationToken, asNoTracking: true);
        return MapDetails(task, _clock.UtcNow);
    }

    public async Task<TaskDetailsDto> UpdateAsync(
        Guid publicId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await GetAccessibleTaskAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(task);

        var related = await ResolveRelatedAsync(
            request.ClientPublicId,
            request.PolicyPublicId,
            request.RenewalPublicId,
            cancellationToken);

        task.Title = request.Title.Trim();
        task.Description = TrimToNull(request.Description);
        task.DueDateUtc = DateTime.SpecifyKind(request.DueDateUtc, DateTimeKind.Utc);
        task.Priority = request.Priority;
        task.ClientId = related.Client?.Id;
        task.PolicyId = related.Policy?.Id;
        task.RenewalId = related.Renewal?.Id;
        task.Client = related.Client;
        task.Policy = related.Policy;
        task.Renewal = related.Renewal;

        AddActivity(task, ActivityType.StatusChanged, $"Task updated: {task.Title}.");
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<TaskDetailsDto> CompleteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var task = await GetAccessibleTaskAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(task);

        task.Status = WorkTaskStatus.Completed;
        task.CompletedAtUtc = _clock.UtcNow;
        AddActivity(task, ActivityType.TaskCompleted, $"Task completed: {task.Title}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<TaskDetailsDto> ReassignAsync(
        Guid publicId,
        ReassignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await GetAccessibleTaskAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(task);

        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserPublicId, cancellationToken);
        var previous = task.AssignedUser?.FullName ?? "Unassigned";
        task.AssignedUserId = assignedUser.Id;
        task.AssignedUser = assignedUser;
        AddActivity(
            task,
            ActivityType.StatusChanged,
            $"Task reassigned from {previous} to {assignedUser.FullName}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<TaskDetailsDto> CancelAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var task = await GetAccessibleTaskAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureOpen(task);

        task.Status = WorkTaskStatus.Cancelled;
        AddActivity(task, ActivityType.StatusChanged, $"Task cancelled: {task.Title}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByPublicIdAsync(publicId, cancellationToken);
    }

    private IQueryable<WorkTask> ApplyView(IQueryable<WorkTask> tasks, string? view, DateTime utcNow)
    {
        return (view?.Trim().ToLowerInvariant()) switch
        {
            "overdue" => tasks.Where(task =>
                OpenStatuses.Contains(task.Status) && task.DueDateUtc < utcNow),
            "completed" => tasks.Where(task => task.Status == WorkTaskStatus.Completed),
            "mine" => tasks.Where(task =>
                task.AssignedUserId == _currentUser.UserId && OpenStatuses.Contains(task.Status)),
            _ => tasks.Where(task => OpenStatuses.Contains(task.Status))
        };
    }

    private IQueryable<WorkTask> AccessibleTasks()
    {
        return _dbContext.Tasks
            .Include(task => task.Client)
            .Include(task => task.Policy)
            .Include(task => task.Renewal)
                .ThenInclude(renewal => renewal!.Policy)
            .Include(task => task.AssignedUser)
            .ForCurrentUser(_currentUser);
    }

    private async Task<WorkTask> GetAccessibleTaskAsync(
        Guid publicId,
        CancellationToken cancellationToken,
        bool asNoTracking)
    {
        var query = AccessibleTasks().Where(task => task.PublicId == publicId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var task = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(task);
        return task!;
    }

    private static void EnsureOpen(WorkTask task)
    {
        if (task.Status is WorkTaskStatus.Completed or WorkTaskStatus.Cancelled)
        {
            throw new BusinessRuleException("This task is already closed.");
        }
    }

    private async Task<User> ResolveAssignedUserAsync(Guid assignedUserPublicId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                entity => entity.PublicId == assignedUserPublicId && entity.IsActive,
                cancellationToken);

        if (user is null)
        {
            throw new ValidationException([
                new ValidationFailure("AssignedUserPublicId", "Assigned user was not found.")
            ]);
        }

        return user;
    }

    private async Task<(Client? Client, Policy? Policy, Renewal? Renewal)> ResolveRelatedAsync(
        Guid? clientPublicId,
        Guid? policyPublicId,
        Guid? renewalPublicId,
        CancellationToken cancellationToken)
    {
        Client? client = null;
        Policy? policy = null;
        Renewal? renewal = null;

        if (clientPublicId.HasValue)
        {
            client = await _dbContext.Clients
                .ForCurrentUser(_currentUser)
                .SingleOrDefaultAsync(entity => entity.PublicId == clientPublicId.Value, cancellationToken);
            if (client is null)
            {
                throw new ValidationException([
                    new ValidationFailure("ClientPublicId", "Client was not found.")
                ]);
            }
        }

        if (policyPublicId.HasValue)
        {
            policy = await _dbContext.Policies
                .Include(entity => entity.Client)
                .ForCurrentUser(_currentUser)
                .SingleOrDefaultAsync(entity => entity.PublicId == policyPublicId.Value, cancellationToken);
            if (policy is null)
            {
                throw new ValidationException([
                    new ValidationFailure("PolicyPublicId", "Policy was not found.")
                ]);
            }

            client ??= policy.Client;
            if (client is not null && policy.ClientId != client.Id)
            {
                throw new ValidationException([
                    new ValidationFailure("PolicyPublicId", "Policy does not belong to the selected client.")
                ]);
            }
        }

        if (renewalPublicId.HasValue)
        {
            renewal = await _dbContext.Renewals
                .Include(entity => entity.Policy)
                    .ThenInclude(entity => entity.Client)
                .ForCurrentUser(_currentUser)
                .SingleOrDefaultAsync(entity => entity.PublicId == renewalPublicId.Value, cancellationToken);
            if (renewal is null)
            {
                throw new ValidationException([
                    new ValidationFailure("RenewalPublicId", "Renewal was not found.")
                ]);
            }

            policy ??= renewal.Policy;
            client ??= renewal.Policy.Client;
            if (policy is not null && renewal.PolicyId != policy.Id)
            {
                throw new ValidationException([
                    new ValidationFailure("RenewalPublicId", "Renewal does not belong to the selected policy.")
                ]);
            }
        }

        return (client, policy, renewal);
    }

    private void AddActivity(WorkTask task, ActivityType activityType, string description)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = task.OrganizationId,
            ClientId = task.ClientId,
            PolicyId = task.PolicyId,
            RenewalId = task.RenewalId,
            UserId = _currentUser.UserId,
            ActivityType = activityType,
            Description = description
        });
    }

    private static IQueryable<WorkTask> ApplySort(IQueryable<WorkTask> query, string? sortBy, bool descending)
    {
        return (sortBy?.Trim().ToLowerInvariant(), descending) switch
        {
            ("title", false) => query.OrderBy(task => task.Title),
            ("title", true) => query.OrderByDescending(task => task.Title),
            ("priority", false) => query.OrderBy(task => task.Priority),
            ("priority", true) => query.OrderByDescending(task => task.Priority),
            ("status", false) => query.OrderBy(task => task.Status),
            ("status", true) => query.OrderByDescending(task => task.Status),
            ("duedateutc", true) => query.OrderByDescending(task => task.DueDateUtc),
            _ => query.OrderBy(task => task.DueDateUtc)
        };
    }

    private static TaskListDto MapList(WorkTask task, DateTime utcNow) =>
        new()
        {
            PublicId = task.PublicId,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            CompletedAtUtc = task.CompletedAtUtc,
            Priority = task.Priority.ToString(),
            Status = EffectiveStatus(task, utcNow).ToString(),
            ClientPublicId = task.Client?.PublicId,
            ClientName = task.Client?.CompanyName,
            PolicyPublicId = task.Policy?.PublicId,
            PolicyNumber = task.Policy?.PolicyNumber,
            RenewalPublicId = task.Renewal?.PublicId,
            AssignedUserPublicId = task.AssignedUser?.PublicId,
            AssignedUserName = task.AssignedUser?.FullName
        };

    private static TaskDetailsDto MapDetails(WorkTask task, DateTime utcNow) =>
        new()
        {
            PublicId = task.PublicId,
            Title = task.Title,
            Description = task.Description,
            DueDateUtc = task.DueDateUtc,
            CompletedAtUtc = task.CompletedAtUtc,
            Priority = task.Priority.ToString(),
            Status = EffectiveStatus(task, utcNow).ToString(),
            ClientPublicId = task.Client?.PublicId,
            ClientName = task.Client?.CompanyName,
            PolicyPublicId = task.Policy?.PublicId,
            PolicyNumber = task.Policy?.PolicyNumber,
            RenewalPublicId = task.Renewal?.PublicId,
            RenewalPolicyNumber = task.Renewal?.Policy.PolicyNumber ?? task.Policy?.PolicyNumber,
            AssignedUserPublicId = task.AssignedUser?.PublicId,
            AssignedUserName = task.AssignedUser?.FullName,
            CreatedAtUtc = task.CreatedAtUtc,
            ModifiedAtUtc = task.ModifiedAtUtc,
            CreatedBy = task.CreatedBy
        };

    private static WorkTaskStatus EffectiveStatus(WorkTask task, DateTime utcNow)
    {
        if (task.Status is WorkTaskStatus.Completed or WorkTaskStatus.Cancelled)
        {
            return task.Status;
        }

        if (task.DueDateUtc < utcNow)
        {
            return WorkTaskStatus.Overdue;
        }

        return task.Status;
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
