namespace BrokerOS.Application.Insurers;

public sealed class InsurerListDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required bool IsActive { get; init; }
}
