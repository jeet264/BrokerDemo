using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class Renewal : Entity, ITenantOwned, IAudited
{
    public long OrganizationId { get; set; }

    public long PolicyId { get; set; }

    public long? AssignedUserId { get; set; }

    public DateOnly RenewalDate { get; set; }

    public RenewalStatus Status { get; set; } = RenewalStatus.Upcoming;

    public RenewalPriority Priority { get; set; } = RenewalPriority.Medium;

    public RenewalStage CurrentStage { get; set; } = RenewalStage.NotStarted;

    public DateTime? LastFollowUpAtUtc { get; set; }

    public DateTime? NextFollowUpAtUtc { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public Organization Organization { get; set; } = null!;

    public Policy Policy { get; set; } = null!;

    public User? AssignedUser { get; set; }

    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
