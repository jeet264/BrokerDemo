namespace BrokerOS.Application.Import;

/// <summary>
/// How a policy spreadsheet row is attached to an existing client in this brokerage.
/// The file never creates clients — unmatched rows are skipped so a typo cannot silently
/// invent a duplicate client. OrganizationId in the file is ignored either way.
/// </summary>
public enum ClientMatchStrategy
{
    /// <summary>Match <c>ClientCode</c> or <c>ClientExternalId</c> to Client.ClientCode (unique per org).</summary>
    ClientCode = 1,

    /// <summary>Match company/client name + phone (digits only) when the spreadsheet has no client code.</summary>
    NameAndPhone = 2
}
