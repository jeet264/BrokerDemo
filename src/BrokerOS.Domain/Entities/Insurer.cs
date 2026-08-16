using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// An insurance company that can underwrite policies.
/// <see cref="OrganizationId"/> null means a system-wide (global) insurer seeded for every tenant to pick from;
/// a non-null value is a brokerage-specific panel entry. Tenants may not edit or delete global insurers.
/// </summary>
public class Insurer : Entity
{
    /// <summary>
    /// Null = global / system insurer visible to every org via the query filter.
    /// Populated = this brokerage's own insurer record. Create always sets the current JWT org.
    /// </summary>
    public long? OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>UTC audit timestamp (not a cover date).</summary>
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public Organization? Organization { get; set; }

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
