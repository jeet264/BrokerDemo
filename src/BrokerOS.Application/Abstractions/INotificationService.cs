using BrokerOS.Application.Notifications;

namespace BrokerOS.Application.Abstractions;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> ListForRenewalAsync(Guid renewalPublicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationDto>> ListAsync(CancellationToken cancellationToken);
}
