namespace BrokerOS.Application.Auth;

/// <summary>Signed-in profile. Role is the enum name (BrokerAdmin / BrokerManager / BrokerEmployee).</summary>
public sealed class CurrentUserDto
{
    public required Guid PublicUserId { get; init; }

    public required string Email { get; init; }

    public required string FullName { get; init; }

    /// <summary>Enum name of <c>UserRole</c>, not the numeric value.</summary>
    public required string Role { get; init; }

    public required Guid OrganizationPublicId { get; init; }

    public required string OrganizationName { get; init; }

    public required string OrganizationCode { get; init; }
}
