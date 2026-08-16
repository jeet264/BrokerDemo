namespace BrokerOS.Domain.Common;

/// <summary>
/// Base type for every persisted BrokerOS record.
/// <see cref="Id"/> is the internal SQL identity (never sent to clients).
/// <see cref="PublicId"/> is the stable GUID used in URLs and JSON so we can
/// change storage later without leaking sequential ids across tenants.
/// </summary>
public abstract class Entity
{
    /// <summary>Internal primary key. Services may use this in-process; APIs expose <see cref="PublicId"/> instead.</summary>
    public long Id { get; set; }

    /// <summary>Public identifier returned by the API and accepted in route parameters.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();
}
