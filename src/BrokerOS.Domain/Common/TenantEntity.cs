namespace BrokerOS.Domain.Common;

public abstract class TenantEntity : Entity
{
    public Guid OrganizationId { get; set; }
}
