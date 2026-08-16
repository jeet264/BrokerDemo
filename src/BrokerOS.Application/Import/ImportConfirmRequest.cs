namespace BrokerOS.Application.Import;

/// <summary>
/// Confirms a previous preview. The UI sends this after the broker reviews the preview grid.
/// Only valid rows are inserted.
/// </summary>
public sealed class ImportConfirmRequest
{
    public Guid PreviewToken { get; set; }
}
