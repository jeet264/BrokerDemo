namespace BrokerOS.Application.Import;

/// <summary>Uploaded spreadsheet. The controller copies IFormFile into a seekable stream so CsvHelper/ClosedXML can rewind.</summary>
public sealed class ImportFileContent
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }
}
