namespace BrokerOS.Domain.Common;

public abstract class Entity
{
    public long Id { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();
}
