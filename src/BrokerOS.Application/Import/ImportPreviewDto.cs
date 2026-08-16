namespace BrokerOS.Application.Import;

/// <summary>
/// Preview payload returned before any rows are saved. <see cref="PreviewToken"/> is handed back
/// to confirm so we do not import in one shot — the broker can see mistakes first.
/// Token is cached in memory (~30 minutes) and is bound to the current OrganizationId.
/// </summary>
public sealed class ImportPreviewDto<TValues>
{
    public required Guid PreviewToken { get; init; }

    public required int TotalRows { get; init; }

    public required int ValidCount { get; init; }

    public required int InvalidCount { get; init; }

    /// <summary>Echo of the match strategy used for policy preview. Null for client imports.</summary>
    public string? MatchStrategy { get; init; }

    public required IReadOnlyList<ImportPreviewRowDto<TValues>> Rows { get; init; }
}
