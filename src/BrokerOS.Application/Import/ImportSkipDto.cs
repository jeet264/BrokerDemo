namespace BrokerOS.Application.Import;

/// <summary>A row that was not written on confirm, with the reason (validation error or a race after preview).</summary>
public sealed class ImportSkipDto
{
    public required int RowNumber { get; init; }

    public required string Reason { get; init; }
}
