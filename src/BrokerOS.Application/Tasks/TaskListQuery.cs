using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Tasks;

public sealed class TaskListQuery
{
    /// <summary>
    /// Desk views: mine, team, overdue, completed.
    /// </summary>
    public string? View { get; set; }

    public string? Search { get; set; }

    public WorkTaskStatus? Status { get; set; }

    public TaskPriority? Priority { get; set; }

    public Guid? AssignedUserPublicId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
