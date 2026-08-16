using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A brokerage tenant. All operational data (users, clients, policies, renewals, tasks, activities)
/// hangs off this row. There is no OrganizationId on Organization itself — tenancy is "this Id".
/// Query filter: only the JWT's OrganizationId is visible, so there is no list-all-tenants API.
/// </summary>
public class Organization : Entity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Short unique code used at registration (normalized uppercase). Not the same as PublicId.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>False blocks every user in the org from logging in (checked in AuthService).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC audit timestamp. Organization is not IAudited because CreatedBy is not stored on this row.</summary>
    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Client> Clients { get; set; } = new List<Client>();

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public ICollection<Insurer> Insurers { get; set; } = new List<Insurer>();

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();

    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
