using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Policies;

public sealed class UpdatePolicyRequest
{
    public string PolicyNumber { get; set; } = string.Empty;

    public Guid ClientPublicId { get; set; }

    public Guid InsurerPublicId { get; set; }

    public PolicyType PolicyType { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public decimal Premium { get; set; }

    public decimal SumInsured { get; set; }

    public decimal CommissionPercentage { get; set; }

    public Guid? AssignedUserPublicId { get; set; }

    public string? Notes { get; set; }
}
