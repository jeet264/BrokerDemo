using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrokerOS.Infrastructure.Notifications;

/// <summary>
/// Demo delivery: attach the notification to the current unit of work as Simulated and do not
/// call any WhatsApp / email / SMS provider. Brokers can still preview the message in-app.
/// </summary>
/// <remarks>
/// Swap this DI registration for a real provider implementation (e.g. WhatsAppBusinessApiSender)
/// when ready to go live — no other code should need to change. A live sender would POST to the
/// WhatsApp Business API (Twilio, Gupshup, Interakt, or Meta Cloud API), then set Status = Sent.
/// </remarks>
public sealed class SimulatedNotificationSender : INotificationSender
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ILogger<SimulatedNotificationSender> _logger;

    public SimulatedNotificationSender(BrokerOsDbContext dbContext, ILogger<SimulatedNotificationSender> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task SendAsync(Notification notification, CancellationToken cancellationToken)
    {
        notification.Status = NotificationStatus.Simulated;
        if (_dbContext.Entry(notification).State == EntityState.Detached)
        {
            _dbContext.Notifications.Add(notification);
        }

        _logger.LogInformation(
            "Simulated {Channel} {RecipientType} reminder for renewal {RenewalId}. Not actually sent.",
            notification.Channel,
            notification.RecipientType,
            notification.RenewalId);

        return Task.CompletedTask;
    }
}
