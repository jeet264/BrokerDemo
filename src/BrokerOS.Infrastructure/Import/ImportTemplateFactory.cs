using ClosedXML.Excel;
using BrokerOS.Application.Import;

namespace BrokerOS.Infrastructure.Import;

/// <summary>
/// Builds the downloadable client/policy templates. Example rows use seeded demo insurers
/// (ICICI Lombard) so a broker can see the expected shape without guessing column names.
/// </summary>
public static class ImportTemplateFactory
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static ImportTemplateFile CreateClientTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Clients");
        var headers = new[]
        {
            "ClientCode", "CompanyName", "ClientType", "Email", "Phone", "Industry",
            "AlternatePhone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "Notes"
        };
        WriteHeader(sheet, headers);

        sheet.Cell(2, 1).Value = "C-1001";
        sheet.Cell(2, 2).Value = "Sunrise Textiles Pvt Ltd";
        sheet.Cell(2, 3).Value = "Corporate";
        sheet.Cell(2, 4).Value = "accounts@sunrise.example";
        sheet.Cell(2, 5).Value = "9876543210";
        sheet.Cell(2, 6).Value = "Manufacturing";
        sheet.Cell(2, 8).Value = "12 MG Road";
        sheet.Cell(2, 10).Value = "Mumbai";
        sheet.Cell(2, 11).Value = "Maharashtra";
        sheet.Cell(2, 12).Value = "400001";
        sheet.Cell(2, 13).Value = "India";

        var help = workbook.AddWorksheet("Instructions");
        help.Cell(1, 1).Value = "BrokerOS client import";
        help.Cell(2, 1).Value = "Required: ClientCode, CompanyName, Phone.";
        help.Cell(3, 1).Value = "ClientType: Corporate, SME, or Individual (blank defaults to Corporate).";
        help.Cell(4, 1).Value = "ClientCode is unique in your brokerage. ClientExternalId is accepted as an alias for ClientCode.";
        help.Cell(5, 1).Value = "An OrganizationId column, if present, is ignored — rows always belong to the signed-in brokerage.";
        help.Cell(6, 1).Value = "Missing address fields are stored as Not provided / 000000 / India so the row can still import.";
        help.Column(1).Width = 110;

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        return Save(workbook, "BrokerOS-clients-template.xlsx");
    }

    public static ImportTemplateFile CreatePolicyTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Policies");
        var headers = new[]
        {
            "PolicyNumber", "ClientCode", "ClientName", "Phone", "InsurerCode", "InsurerName",
            "PolicyType", "StartDate", "ExpiryDate", "Premium", "SumInsured",
            "CommissionPercentage", "CommissionAmount", "Status", "Notes"
        };
        WriteHeader(sheet, headers);

        sheet.Cell(2, 1).Value = "POL-2026-001";
        sheet.Cell(2, 2).Value = "C-1001";
        sheet.Cell(2, 3).Value = "Sunrise Textiles Pvt Ltd";
        sheet.Cell(2, 4).Value = "9876543210";
        sheet.Cell(2, 5).Value = "ICICIL";
        sheet.Cell(2, 6).Value = "ICICI Lombard";
        sheet.Cell(2, 7).Value = "Property";
        sheet.Cell(2, 8).Value = "2026-04-01";
        sheet.Cell(2, 8).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.Cell(2, 9).Value = "2027-03-31";
        sheet.Cell(2, 9).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.Cell(2, 10).Value = 125000;
        sheet.Cell(2, 11).Value = 50000000;
        sheet.Cell(2, 12).Value = 12.5;
        sheet.Cell(2, 14).Value = "Active";

        var help = workbook.AddWorksheet("Instructions");
        help.Cell(1, 1).Value = "BrokerOS policy import";
        help.Cell(2, 1).Value = "Required: PolicyNumber, PolicyType, StartDate, ExpiryDate, Premium, plus a client match column.";
        help.Cell(3, 1).Value = "Match by ClientCode (or ClientExternalId) — or choose Name + Phone on the import screen and fill ClientName and Phone.";
        help.Cell(4, 1).Value = "InsurerCode or InsurerName must match an insurer already on the panel (including system insurers).";
        help.Cell(5, 1).Value = "Dates: yyyy-MM-dd or dd/MM/yyyy. Premium: number (₹ and commas are stripped).";
        help.Cell(6, 1).Value = "PolicyType: Property, Marine, Engineering, Liability, Motor, Health, EmployeeBenefits, Other.";
        help.Cell(7, 1).Value = "An OrganizationId column, if present, is ignored.";
        help.Column(1).Width = 120;

        StyleHeader(sheet, headers.Length);
        sheet.Columns().AdjustToContents();
        return Save(workbook, "BrokerOS-policies-template.xlsx");
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }
    }

    private static void StyleHeader(IXLWorksheet sheet, int columnCount)
    {
        var range = sheet.Range(1, 1, 1, columnCount);
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#0b2b43");
        range.Style.Font.FontColor = XLColor.White;
        sheet.SheetView.FreezeRows(1);
    }

    private static ImportTemplateFile Save(XLWorkbook workbook, string downloadName)
    {
        using var buffer = new MemoryStream();
        workbook.SaveAs(buffer);
        return new ImportTemplateFile
        {
            Content = buffer.ToArray(),
            ContentType = ExcelContentType,
            DownloadName = downloadName
        };
    }
}
