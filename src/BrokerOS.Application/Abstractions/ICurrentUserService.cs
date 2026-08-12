using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Abstractions;

public interface ICurrentUserService
{
    long UserId { get; }

    Guid PublicUserId { get; }

    long OrganizationId { get; }

    UserRole Role { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}
