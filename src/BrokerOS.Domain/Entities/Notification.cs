using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class Notification : Entity, ITenantOwned
{
    public long OrganizationId { get; set; }

    public long RenewalId { get; set; }

    public long? ClientId { get; set; }

    public NotificationRecipientType RecipientType { get; set; }

    public NotificationChannel Channel { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Simulated;

    public int? ReminderMilestoneDays { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public Renewal Renewal { get; set; } = null!;

    public Client? Client { get; set; }
}
