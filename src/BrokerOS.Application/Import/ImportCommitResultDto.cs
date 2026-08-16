namespace BrokerOS.Application.Import;

/// <summary>Summary after confirm. Only rows that were valid at commit time are imported.</summary>
public sealed class ImportCommitResultDto
{
    public required int ImportedCount { get; init; }

    public required int SkippedCount { get; init; }

    public required IReadOnlyList<ImportSkipDto> Skipped { get; init; }
}
