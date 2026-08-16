using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

public class Insurer : Entity
{
    public long? OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public Organization? Organization { get; set; }

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
