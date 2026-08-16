using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A buyer of insurance at this brokerage (corporate, SME, or individual).
/// The book of business hangs off this record: contacts, policy terms, tasks, and activity.
/// Delete is soft (<see cref="IsDeleted"/>) so historical policies remain attributable.
/// </summary>
public class Client : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    /// <summary>Owning brokerage. Set from the JWT on create — never accepted from the request body as a tenant key.</summary>
    public long OrganizationId { get; set; }

    /// <summary>Human-facing code unique among non-deleted clients in this organization (not globally).</summary>
    public string ClientCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public ClientType ClientType { get; set; }

    public string? Industry { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? AlternatePhone { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = "India";

    /// <summary>Null = unassigned. BrokerEmployees only see clients where this equals their user id.</summary>
    public long? AssignedUserId { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Operational flag (paused vs active book). Distinct from <see cref="IsDeleted"/>:
    /// inactive clients still list; deleted clients are hidden by the query filter.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Audit timestamp (UTC), not a business date.</summary>
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;

    public User? AssignedUser { get; set; }

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
