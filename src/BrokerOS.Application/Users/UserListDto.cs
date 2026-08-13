namespace BrokerOS.Application.Users;

public sealed class UserListDto
{
    public required Guid PublicId { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required string Role { get; init; }

    public required bool IsActive { get; init; }
}
