using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class Activity : Entity, ITenantOwned
{
    public long OrganizationId { get; set; }

    public long? ClientId { get; set; }

    public long? PolicyId { get; set; }

    public long? RenewalId { get; set; }

    public long UserId { get; set; }

    public ActivityType ActivityType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client? Client { get; set; }

    public Policy? Policy { get; set; }

    public Renewal? Renewal { get; set; }

    public User User { get; set; } = null!;
}
