using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Abstractions;

/// <summary>
/// The signed-in user as JWT claims. OrganizationId is the tenant key for all scoped queries.
/// </summary>
public interface ICurrentUserService
{
    long UserId { get; }

    Guid PublicUserId { get; }

    long OrganizationId { get; }

    UserRole Role { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}
