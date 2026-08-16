using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A follow-up or work item for a broker user. Optional FKs let a task hang off a renewal,
/// a client, a policy, or any combination — at least the title and due instant are required.
/// Soft-deleted so completed/cancelled work can leave the live list without erasing history.
/// </summary>
public class WorkTask : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    public long OrganizationId { get; set; }

    /// <summary>Null when the task is not tied to a specific renewal (e.g. a general client chase).</summary>
    public long? RenewalId { get; set; }

    /// <summary>Null when the task is not about a client (rare; usually populated from the renewal's client).</summary>
    public long? ClientId { get; set; }

    /// <summary>Null when the task is not about a specific policy term.</summary>
    public long? PolicyId { get; set; }

    /// <summary>Null = unassigned. Employees only see tasks assigned to them.</summary>
    public long? AssignedUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>When the work is due. UTC DateTime because it is a reminder instant (display in IST), not a cover DateOnly.</summary>
    public DateTime DueDateUtc { get; set; }

    /// <summary>Set when Status becomes Completed. Null means not done. UTC audit timestamp.</summary>
    public DateTime? CompletedAtUtc { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public Renewal? Renewal { get; set; }

    public Client? Client { get; set; }

    public Policy? Policy { get; set; }

    public User? AssignedUser { get; set; }
}
