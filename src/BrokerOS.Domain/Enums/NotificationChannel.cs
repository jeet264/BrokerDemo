namespace BrokerOS.Domain.Enums;

/// <summary>
/// How the message would go out. WhatsApp is the primary client channel.
/// Email is for internal/insurer messages. SMS is retained for a future provider but is not
/// the default for new client reminders.
/// </summary>
public enum NotificationChannel
{
    Email = 1,
    WhatsApp = 2,
    SMS = 3
}
