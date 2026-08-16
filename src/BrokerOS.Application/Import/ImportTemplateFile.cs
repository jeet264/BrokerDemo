namespace BrokerOS.Application.Import;

/// <summary>Generated .xlsx template bytes for download. Not tenant-specific.</summary>
public sealed class ImportTemplateFile
{
    public required byte[] Content { get; init; }

    public required string ContentType { get; init; }

    public required string DownloadName { get; init; }
}
