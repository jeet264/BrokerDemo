using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;

namespace BrokerOS.Application.Security;

public static class AssignmentScope
{
    public static IQueryable<Client> ForCurrentUser(this IQueryable<Client> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(client => client.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static IQueryable<Policy> ForCurrentUser(this IQueryable<Policy> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(policy => policy.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static IQueryable<Renewal> ForCurrentUser(this IQueryable<Renewal> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(renewal => renewal.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static IQueryable<WorkTask> ForCurrentUser(this IQueryable<WorkTask> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(task => task.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static IQueryable<Notification> ForCurrentUser(this IQueryable<Notification> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(notification => notification.Renewal.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static IQueryable<Quotation> ForCurrentUser(this IQueryable<Quotation> query, ICurrentUserService currentUser)
    {
        if (currentUser.Role == UserRole.BrokerEmployee)
        {
            return query.Where(quotation => quotation.Renewal.AssignedUserId == currentUser.UserId);
        }

        return query;
    }

    public static void EnsureCanAccessAssigned(ICurrentUserService currentUser, long? assignedUserId)
    {
        if (currentUser.Role is UserRole.BrokerAdmin or UserRole.BrokerManager)
        {
            return;
        }

        if (currentUser.Role == UserRole.BrokerEmployee && assignedUserId == currentUser.UserId)
        {
            return;
        }

        throw new NotFoundException("The requested resource was not found.");
    }

    public static void EnsureFound<T>(T? entity)
    {
        if (entity is null)
        {
            throw new NotFoundException("The requested resource was not found.");
        }
    }
}
