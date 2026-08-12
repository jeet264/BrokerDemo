namespace BrokerOS.Application.Insurers;

public sealed class InsurerDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? Website { get; init; }

    public required bool IsActive { get; init; }

    public required bool IsGlobal { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }
}
