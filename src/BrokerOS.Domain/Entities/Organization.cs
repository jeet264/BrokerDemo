using BrokerOS.Domain.Common;

namespace BrokerOS.Domain.Entities;

public class Organization : Entity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

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
