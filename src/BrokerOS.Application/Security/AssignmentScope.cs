using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;

namespace BrokerOS.Application.Security;

/// <summary>
/// Second fence on top of tenant query filters: BrokerEmployees see only rows assigned to them.
/// Admins and managers see the full org book. Out-of-scope access is 404 (not 403) so we do not
/// confirm that another user's or tenant's id exists.
/// </summary>
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

    /// <summary>
    /// Throws NotFoundException when an employee is not the assignee. Admins/managers always pass.
    /// 404 rather than 403: leaking "forbidden" would confirm the record exists outside their book.
    /// </summary>
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

    /// <summary>Null after a tenant- and assignment-filtered query means 404, not "empty success".</summary>
    public static void EnsureFound<T>(T? entity)
    {
        if (entity is null)
        {
            throw new NotFoundException("The requested resource was not found.");
        }
    }
}
