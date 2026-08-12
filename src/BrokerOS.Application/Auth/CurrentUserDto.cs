namespace BrokerOS.Application.Auth;

public sealed class CurrentUserDto
{
    public required Guid PublicUserId { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    public required string Role { get; init; }

    public required Guid OrganizationPublicId { get; init; }

    public required string OrganizationName { get; init; }

    public required string OrganizationCode { get; init; }
}
