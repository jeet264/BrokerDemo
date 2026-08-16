namespace BrokerOS.Application.Import;

/// <summary>
/// One spreadsheet row after parse + validation. Invalid rows stay in the preview so the broker
/// can fix the file; they are never written on confirm.
/// </summary>
public sealed class ImportPreviewRowDto<TValues>
{
    /// <summary>1-based row number in the file including the header (header is row 1, first data row is 2).</summary>
    public required int RowNumber { get; init; }

    public required bool IsValid { get; init; }

    /// <summary>Set when <see cref="IsValid"/> is false. Plain-English reason (missing phone, duplicate policy number, …).</summary>
    public string? Error { get; init; }

    public required TValues Values { get; init; }
}
