using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// Tracks the work of renewing one policy term before it expires.
/// Completing a renewal (Prompt 7B) must insert a new <see cref="Policy"/> for the next term
/// and mark the old policy Expired — it must not stretch this policy's ExpiryDate.
/// Renewals are not soft-deleted: a lost or cancelled attempt stays on the timeline.
/// </summary>
public class Renewal : Entity, ITenantOwned, IAudited
{
    /// <summary>Owning brokerage. Copied from the policy / JWT tenant context.</summary>
    public long OrganizationId { get; set; }

    public long PolicyId { get; set; }

    /// <summary>Null means unassigned. Employees only see renewals assigned to them.</summary>
    public long? AssignedUserId { get; set; }

    /// <summary>
    /// Calendar date the cover must be renewed by (usually the policy ExpiryDate).
    /// DateOnly: this is a business date, not a follow-up timestamp.
    /// </summary>
    public DateOnly RenewalDate { get; set; }

    /// <summary>
    /// Pipeline outcome. Upcoming / InProgress / quotation / decision states are still this term;
    /// Renewed means a new Policy exists (or will, once rollover is implemented);
    /// Lost / Cancelled close the attempt without a new term;
    /// Overdue means RenewalDate has passed while still open.
    /// </summary>
    public RenewalStatus Status { get; set; } = RenewalStatus.Upcoming;

    public RenewalPriority Priority { get; set; } = RenewalPriority.Medium;

    /// <summary>Where the broker is in the operational checklist (contact → quote → decision → done). Independent of <see cref="Status"/> so a case can be InProgress at QuotationReceived.</summary>
    public RenewalStage CurrentStage { get; set; } = RenewalStage.NotStarted;

    /// <summary>When the last follow-up actually happened. UTC DateTime because it is an event instant, not a cover date.</summary>
    public DateTime? LastFollowUpAtUtc { get; set; }

    /// <summary>When the next chase is due. UTC DateTime; display in IST in the UI. Null = no reminder scheduled.</summary>
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
