namespace BrokerOS.Application.Insurers;

/// <summary>Insurer list row. IsGlobal is derived from OrganizationId == null (not a stored column).</summary>
public sealed class InsurerListDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? Website { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>True when this is a system insurer (OrganizationId is null). Tenants cannot edit or delete these.</summary>
    public required bool IsGlobal { get; init; }
}
