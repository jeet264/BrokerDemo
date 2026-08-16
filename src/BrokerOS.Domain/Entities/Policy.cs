using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class Policy : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    public long OrganizationId { get; set; }

    public long ClientId { get; set; }

    public long InsurerId { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public PolicyType PolicyType { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public decimal Premium { get; set; }

    public decimal SumInsured { get; set; }

    public decimal CommissionPercentage { get; set; }

    public decimal CommissionAmount { get; set; }

    public long? AssignedUserId { get; set; }

    public PolicyStatus Status { get; set; } = PolicyStatus.Active;

    public long? PreviousPolicyId { get; set; }

    public long? NextPolicyId { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Motor registration (e.g. MH-01-AB-1234). Optional; other policy types leave this empty.
    /// </summary>
    public string? VehicleNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client Client { get; set; } = null!;

    public Insurer Insurer { get; set; } = null!;

    public User? AssignedUser { get; set; }

    public Policy? PreviousPolicy { get; set; }

    public Policy? NextPolicy { get; set; }

    public ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();

    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
