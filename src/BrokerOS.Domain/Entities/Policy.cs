using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// Represents a single insurance policy term for a client.
/// When a policy is renewed, a NEW Policy record is created for the next term
/// (future <c>RenewalService.CompleteRenewalAsync</c>) rather than mutating this one —
/// this one is marked <see cref="PolicyStatus.Expired"/>. PreviousPolicyId / NextPolicyId
/// will link the chain for history once Prompt 7B adds those columns. Until then, do not
/// overwrite StartDate / ExpiryDate / Premium on an expired term: those values are the audit trail.
/// </summary>
public class Policy : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    /// <summary>Owning brokerage. Always set from the JWT tenant context, never from a client-supplied id.</summary>
    public long OrganizationId { get; set; }

    public long ClientId { get; set; }

    public long InsurerId { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public PolicyType PolicyType { get; set; }

    /// <summary>
    /// First day of cover. <see cref="DateOnly"/> because this is a business/calendar date,
    /// not an audit timestamp — storing DateTime would invite timezone bugs around IST midnight.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Last day of cover for this term. DateOnly for the same reason as <see cref="StartDate"/>.
    /// The related <see cref="Renewal.RenewalDate"/> is typically this date.
    /// </summary>
    public DateOnly ExpiryDate { get; set; }

    /// <summary>Gross premium for this term. SQL decimal(18,2).</summary>
    public decimal Premium { get; set; }

    /// <summary>Sum insured for this term. SQL decimal(18,2).</summary>
    public decimal SumInsured { get; set; }

    /// <summary>Broker commission rate for this term. SQL decimal(18,4). Stored on the term so later % changes do not rewrite history.</summary>
    public decimal CommissionPercentage { get; set; }

    /// <summary>
    /// Commission in currency for this term. Stored (not only computed) so historical terms
    /// keep the amount that was earned even if the calculation rule changes later.
    /// </summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>Null means the policy is unassigned. Employees cannot see unassigned policies (AssignmentScope).</summary>
    public long? AssignedUserId { get; set; }

    /// <summary>
    /// Active = current term; PendingRenewal = still this term but a renewal is in flight;
    /// Expired = this term is closed — look for a newer Policy (NextPolicyId, once added) rather than editing this row;
    /// Cancelled = cover ended without a rollover.
    /// </summary>
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;

    public string? Notes { get; set; }

    /// <summary>Audit timestamp (UTC). Not a cover date — do not change to DateOnly.</summary>
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client Client { get; set; } = null!;

    public Insurer Insurer { get; set; } = null!;

    public User? AssignedUser { get; set; }

    public ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();

    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
