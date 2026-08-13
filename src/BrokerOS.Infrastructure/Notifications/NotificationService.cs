using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Notifications;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(BrokerOsDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> ListForRenewalAsync(
        Guid renewalPublicId,
        CancellationToken cancellationToken)
    {
        var renewal = await _dbContext.Renewals
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == renewalPublicId, cancellationToken);
        AssignmentScope.EnsureFound(renewal);

        var notifications = await AccessibleNotifications()
            .Where(notification => notification.RenewalId == renewal!.Id)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return notifications.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> ListAsync(CancellationToken cancellationToken)
    {
        var notifications = await AccessibleNotifications()
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        return notifications.Select(Map).ToList();
    }

    private IQueryable<Notification> AccessibleNotifications()
    {
        return _dbContext.Notifications
            .AsNoTracking()
            .Include(notification => notification.Client)
            .Include(notification => notification.Renewal)
                .ThenInclude(renewal => renewal.Organization)
            .Include(notification => notification.Renewal)
                .ThenInclude(renewal => renewal.AssignedUser)
            .Include(notification => notification.Renewal)
                .ThenInclude(renewal => renewal.Policy)
                    .ThenInclude(policy => policy.Client)
            .Include(notification => notification.Renewal)
                .ThenInclude(renewal => renewal.Policy)
                    .ThenInclude(policy => policy.Insurer)
            .Include(notification => notification.Renewal)
                .ThenInclude(renewal => renewal.Policy)
                    .ThenInclude(policy => policy.AssignedUser)
            .ForCurrentUser(_currentUser);
    }

    private static NotificationDto Map(Notification notification)
    {
        var renewal = notification.Renewal;
        var policy = renewal.Policy;
        var client = notification.Client ?? policy.Client;
        var insurer = policy.Insurer;
        var assigned = renewal.AssignedUser ?? policy.AssignedUser;
        var (recipientName, recipientAddress) = ResolveRecipient(notification, client, insurer, assigned);

        return new NotificationDto
        {
            PublicId = notification.PublicId,
            RenewalPublicId = renewal.PublicId,
            ClientPublicId = client?.PublicId,
            ClientName = client?.CompanyName,
            PolicyNumber = policy.PolicyNumber,
            OrganizationName = renewal.Organization.Name,
            RecipientType = notification.RecipientType.ToString(),
            Channel = notification.Channel.ToString(),
            RecipientName = recipientName,
            RecipientAddress = recipientAddress,
            Subject = notification.Subject,
            Body = notification.Body,
            Status = notification.Status.ToString(),
            ReminderMilestoneDays = notification.ReminderMilestoneDays,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }

    private static (string Name, string? Address) ResolveRecipient(
        Notification notification,
        Client? client,
        Insurer? insurer,
        User? assigned)
    {
        return notification.RecipientType switch
        {
            NotificationRecipientType.Client => (
                client?.CompanyName ?? "Client",
                notification.Channel == NotificationChannel.Email ? client?.Email : client?.Phone),
            NotificationRecipientType.Insurer => (
                insurer?.Name ?? "Insurer",
                notification.Channel == NotificationChannel.Email ? insurer?.Email : insurer?.Phone),
            NotificationRecipientType.InternalUser => (
                assigned?.FullName ?? "Internal",
                assigned?.Email),
            _ => ("Recipient", null)
        };
    }
}
