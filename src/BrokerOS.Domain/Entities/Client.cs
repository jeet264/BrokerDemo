using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class Client : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    public long OrganizationId { get; set; }

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

    public long? AssignedUserId { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

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
