using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A person at a <see cref="Client"/> (decision maker, accounts, etc.).
/// Soft-deleted with the rest of the address book. Not IAudited — only created/modified timestamps, no CreatedBy.
/// </summary>
public class Contact : Entity, ITenantOwned, ISoftDeletable
{
    public long OrganizationId { get; set; }

    public long ClientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Designation { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>True for the default person to call. Multiple primaries are not currently prevented at the database level.</summary>
    public bool IsPrimary { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client Client { get; set; } = null!;
}
