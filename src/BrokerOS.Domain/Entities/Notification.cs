using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// An outbound reminder the renewal worker (or a future "send now" action) asked to deliver.
/// Status stays Simulated until a live <c>INotificationSender</c> is registered and actually posts
/// to a provider. Channel defaults to WhatsApp for client-facing copy; email is opt-in for
/// internal/insurer messages.
/// </summary>
public class Notification : Entity, ITenantOwned
{
    public long OrganizationId { get; set; }

    public long RenewalId { get; set; }

    public long? ClientId { get; set; }

    public NotificationRecipientType RecipientType { get; set; }

    /// <summary>
    /// WhatsApp is the default because that is how Indian brokers chase clients.
    /// Set Email explicitly for internal desk notes or insurer quotation requests.
    /// </summary>
    public NotificationChannel Channel { get; set; } = NotificationChannel.WhatsApp;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Simulated;

    public int? ReminderMilestoneDays { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public Renewal Renewal { get; set; } = null!;

    public Client? Client { get; set; }
}
