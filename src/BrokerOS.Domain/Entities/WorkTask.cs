using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class WorkTask : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    public long OrganizationId { get; set; }

    public long? RenewalId { get; set; }

    public long? ClientId { get; set; }

    public long? PolicyId { get; set; }

    public long? AssignedUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDateUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

    public int? ReminderMilestoneDays { get; set; }

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
