using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Renewals;

public sealed class RenewalListQuery
{
    public string? Search { get; set; }

    public RenewalStatus? Status { get; set; }

    public RenewalStage? Stage { get; set; }

    public RenewalPriority? Priority { get; set; }

    public Guid? AssignedUserPublicId { get; set; }

    public Guid? ClientPublicId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public int? DueWithinDays { get; set; }

    /// <summary>
    /// Preset desk views: all (open), overdue, dueToday, dueIn7Days, dueIn30Days, completed, lost.
    /// </summary>
    public string? DueFilter { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
