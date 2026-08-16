using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using BrokerOS.Domain.Exceptions;

namespace BrokerOS.Infrastructure.Import;

public sealed class SpreadsheetTable
{
    public required IReadOnlyList<string> Headers { get; init; }

    public required IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; init; }
}

/// <summary>
/// Turns a CSV or XLSX upload into a list of row dictionaries keyed by normalized header
/// (lowercase, no spaces/underscores). Old .xls (BIFF) is not supported — brokers should save as .xlsx.
/// </summary>
public static class SpreadsheetReader
{
    public const int MaxDataRows = 2000;

    public static SpreadsheetTable Read(Stream content, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => ReadCsv(content),
            ".xlsx" => ReadXlsx(content),
            ".xls" => throw new BusinessRuleException("Excel 97-2003 (.xls) files are not supported. Save the workbook as .xlsx or export CSV."),
            _ => throw new BusinessRuleException("Upload a .csv or .xlsx file.")
        };
    }

    private static SpreadsheetTable ReadCsv(Stream content)
    {
        using var reader = new StreamReader(content, leaveOpen: true);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            IgnoreBlankLines = true
        });

        if (!csv.Read() || !csv.ReadHeader() || csv.HeaderRecord is null || csv.HeaderRecord.Length == 0)
        {
            throw new BusinessRuleException("The file has no header row.");
        }

        var headers = csv.HeaderRecord.Select(NormalizeHeader).Where(header => header.Length > 0).ToArray();
        var rows = new List<IReadOnlyDictionary<string, string>>();

        while (csv.Read())
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < csv.HeaderRecord.Length; i++)
            {
                var header = NormalizeHeader(csv.HeaderRecord[i]);
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                values[header] = csv.GetField(i)?.Trim() ?? string.Empty;
            }

            if (values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            rows.Add(values);
            EnsureRowLimit(rows.Count);
        }

        return new SpreadsheetTable { Headers = headers, Rows = rows };
    }

    private static SpreadsheetTable ReadXlsx(Stream content)
    {
        using var workbook = new XLWorkbook(content);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new BusinessRuleException("The workbook has no worksheets.");

        var firstRowUsed = worksheet.FirstRowUsed()
            ?? throw new BusinessRuleException("The file has no header row.");

        var headerRow = firstRowUsed.RowNumber();
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRow;

        var headersByColumn = new Dictionary<int, string>();
        for (var column = 1; column <= lastColumn; column++)
        {
            var header = NormalizeHeader(worksheet.Cell(headerRow, column).GetString());
            if (!string.IsNullOrWhiteSpace(header))
            {
                headersByColumn[column] = header;
            }
        }

        if (headersByColumn.Count == 0)
        {
            throw new BusinessRuleException("The file has no header row.");
        }

        var rows = new List<IReadOnlyDictionary<string, string>>();
        for (var rowNumber = headerRow + 1; rowNumber <= lastRow; rowNumber++)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (column, header) in headersByColumn)
            {
                values[header] = CellToString(worksheet.Cell(rowNumber, column));
            }

            if (values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            rows.Add(values);
            EnsureRowLimit(rows.Count);
        }

        return new SpreadsheetTable
        {
            Headers = headersByColumn.Values.ToArray(),
            Rows = rows
        };
    }

    /// <summary>
    /// Date cells become yyyy-MM-dd (DateOnly-friendly). Numbers stay invariant so premium
    /// does not pick up the server's thousands separator.
    /// </summary>
    private static string CellToString(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return string.Empty;
        }

        return cell.DataType switch
        {
            XLDataType.DateTime => DateOnly.FromDateTime(cell.GetDateTime()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            XLDataType.Number => cell.GetDouble().ToString(CultureInfo.InvariantCulture),
            XLDataType.Boolean => cell.GetBoolean() ? "true" : "false",
            _ => cell.GetString().Trim()
        };
    }

    public static string NormalizeHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        var chars = header.Trim().Where(ch => ch is not (' ' or '_' or '-' or '/')).ToArray();
        return new string(chars).ToLowerInvariant();
    }

    public static string Get(IReadOnlyDictionary<string, string> row, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (row.TryGetValue(alias, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    public static bool HasHeader(IReadOnlyList<string> headers, params string[] aliases)
    {
        var keys = headers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return aliases.Any(keys.Contains);
    }

    private static void EnsureRowLimit(int count)
    {
        if (count > MaxDataRows)
        {
            throw new BusinessRuleException($"The file has more than {MaxDataRows} data rows. Split it into smaller files.");
        }
    }
}
