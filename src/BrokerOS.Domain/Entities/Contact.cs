using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

public class Contact : Entity, ITenantOwned, ISoftDeletable
{
    public long OrganizationId { get; set; }

    public long ClientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Designation { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client Client { get; set; } = null!;
}
