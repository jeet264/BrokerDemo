namespace BrokerOS.Application.Insurers;

/// <summary>Insurer detail. IsGlobal is derived from OrganizationId == null.</summary>
public sealed class InsurerDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? Website { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>True when this is a system insurer (OrganizationId is null).</summary>
    public required bool IsGlobal { get; init; }

    /// <summary>UTC audit timestamp.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }
}
